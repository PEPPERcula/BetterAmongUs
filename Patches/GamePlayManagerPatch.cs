using HarmonyLib;
using System.Text;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Patches;

class GamePlayManager
{
    [HarmonyPatch(typeof(LobbyBehaviour))]
    public class LobbyBehaviourPatch
    {
        [HarmonyPatch(nameof(LobbyBehaviour.OnDestroy))]
        [HarmonyPrefix]
        private static void OnDestroy_Prefix(/*LobbyBehaviour __instance*/)
        {
            if (GameStates.IsInGame)
            {
                AntiCheat.PauseAntiCheat();
            }
        }
        [HarmonyPatch(nameof(LobbyBehaviour.Start))]
        [HarmonyPostfix]
        private static void Start_Postfix(/*LobbyBehaviour __instance*/)
        {
            _ = new LateTask(() =>
            {
                if (GameStates.IsInGame)
                {
                    RPC.SyncAllNames(force: true);
                    RPC.SendBetterCheck();
                }
            }, 1.5f, "LobbyBehaviourPatch SyncAllNames");
        }

        // Disabled annoying music
        [HarmonyPatch(nameof(LobbyBehaviour.Update))]
        [HarmonyPostfix]
        public static void Update_Postfix(/*LobbyBehaviour __instance*/)
        {
            if (Main.DisableLobbyTheme.Value)
                SoundManager.instance.StopSound(LobbyBehaviour.Instance.MapTheme);

            // Clear unused Better Data
            try
            {
                var keysToRemove = new List<string>();

                foreach (var betterInfo in PlayerControlDataExtension.playerInfo)
                {
                    if (Main.AllPlayerControls.Any(pc => pc.Data.Puid == betterInfo.Key))
                        continue;

                    keysToRemove.Add(betterInfo.Key);
                }

                foreach (var key in keysToRemove)
                {
                    PlayerControlDataExtension.playerInfo.Remove(key);
                }
            }
            catch { }
        }

        [HarmonyPatch(nameof(LobbyBehaviour.RpcExtendLobbyTimer))]
        [HarmonyPostfix]
        private static void RpcExtendLobbyTimer_Postfix(/*LobbyBehaviour __instance*/)
        {
            GameStartManagerPatch.lobbyTimer += 30f;
        }
    }

    [HarmonyPatch(typeof(IntroCutscene))]
    public class IntroCutscenePatch
    {
        [HarmonyPatch(nameof(IntroCutscene.ShowRole))]
        [HarmonyPostfix]
        private static void ShowRole_Postfix(IntroCutscene __instance)
        {
            try
            {
                static Color HexToColor(string hex)
                {
                    if (hex.StartsWith("#"))
                    {
                        hex = hex.Substring(1);
                    }

                    byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                    byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

                    return new Color32(r, g, b, 255);
                }

                _ = new LateTask(() =>
                {
                    Color RoleColor = HexToColor(Utils.GetRoleColor(PlayerControl.LocalPlayer.Data.RoleType));

                    __instance.ImpostorText.gameObject.SetActive(false);
                    __instance.TeamTitle.gameObject.SetActive(false);
                    __instance.BackgroundBar.material.color = RoleColor;
                    __instance.BackgroundBar.transform.SetLocalZ(-15);
                    __instance.transform.Find("BackgroundLayer").transform.SetLocalZ(-16);
                    __instance.YouAreText.color = RoleColor;
                    __instance.RoleText.color = RoleColor;
                    __instance.RoleBlurbText.color = RoleColor;
                }, 0.0025f, shoudLog: false);
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(GameManager))]
    public class GameManagerPatch
    {
        [HarmonyPatch(nameof(GameManager.EndGame))]
        [HarmonyPostfix]
        private static void EndGame_Postfix(/*GameManager __instance*/)
        {
            if (GameStates.IsHost)
            {
                foreach (PlayerControl player in Main.AllPlayerControls)
                {
                    player.RpcSetName(player.Data.PlayerName);
                }
            }
        }
    }

    [HarmonyPatch(typeof(GameStartManager))]
    public class GameStartManagerPatch
    {
        public static float lobbyTimer = 600f;
        public static string lobbyTimerDisplay = "";
        [HarmonyPatch(nameof(GameStartManager.Start))]
        [HarmonyPostfix]
        private static void Start_Postfix(/*GameStartManager __instance*/)
        {
            lobbyTimer = 600f;
        }
        [HarmonyPatch(nameof(GameStartManager.Update))]
        [HarmonyPrefix]
        private static void Update_Prefix(GameStartManager __instance)
        {
            lobbyTimer = Mathf.Max(0f, lobbyTimer -= Time.deltaTime);
            int minutes = (int)lobbyTimer / 60;
            int seconds = (int)lobbyTimer % 60;
            lobbyTimerDisplay = $"{minutes:00}:{seconds:00}";

            __instance.MinPlayers = 1;
        }
        [HarmonyPatch(nameof(GameStartManager.Update))]
        [HarmonyPostfix]
        private static void Update_Postfix(GameStartManager __instance)
        {
            if (!GameStates.IsHost)
            {
                __instance.StartButton.gameObject.SetActive(false);
                return;

            }
            __instance.GameStartTextParent.SetActive(false);
            __instance.StartButton.gameObject.SetActive(true);
            if (__instance.startState == GameStartManager.StartingStates.Countdown)
            {
                __instance.StartButton.buttonText.text = string.Format("Cancel: {0}", (int)__instance.countDownTimer + 1);
            }
            else
            {
                __instance.StartButton.buttonText.text = "Start Game";
            }
        }
        [HarmonyPatch(nameof(GameStartManager.BeginGame))]
        [HarmonyPrefix]
        private static bool BeginGame_Prefix(GameStartManager __instance)
        {
            if (__instance.startState == GameStartManager.StartingStates.Countdown)
            {
                SoundManager.instance.StopSound(__instance.gameStartSound);
                __instance.ResetStartState();
                return false;
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                __instance.startState = GameStartManager.StartingStates.Countdown;
                __instance.FinallyBegin();
                return false;
            }

            return true;
        }

        [HarmonyPatch(nameof(GameStartManager.FinallyBegin))]
        [HarmonyPrefix]
        private static void FinallyBegin_Prefix(/*GameStartManager __instance*/)
        {
            Logger.LogHeader($"Game Has Started - {Enum.GetName(typeof(MapNames), GameStates.GetActiveMapId)}/{GameStates.GetActiveMapId}", "GamePlayManager");
        }
    }
    [HarmonyPatch(typeof(EndGameManager))]
    public class EndGameManagerPatch
    {
        [HarmonyPatch(nameof(EndGameManager.SetEverythingUp))]
        [HarmonyPostfix]
        private static void SetEverythingUp_Postfix(EndGameManager __instance)
        {
            Logger.LogHeader($"Game Has Ended - {Enum.GetName(typeof(MapNames), GameStates.GetActiveMapId)}/{GameStates.GetActiveMapId}", "GamePlayManager");

            Logger.LogHeader($"Game Summary Start", "GameSummary");

            GameObject SummaryObj = UnityEngine.Object.Instantiate(__instance.WinText.gameObject, __instance.WinText.transform.parent.transform);
            SummaryObj.name = "SummaryObj (TMP)";
            SummaryObj.transform.SetSiblingIndex(0);
            Camera localCamera;
            if (DestroyableSingleton<HudManager>.InstanceExists)
            {
                localCamera = DestroyableSingleton<HudManager>.Instance.GetComponentInChildren<Camera>();
            }
            else
            {
                localCamera = Camera.main;
            }

            SummaryObj.transform.position = AspectPosition.ComputeWorldPosition(localCamera, AspectPosition.EdgeAlignments.LeftTop, new Vector3(1f, 0.2f, -5f));
            SummaryObj.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
            TextMeshPro SummaryText = SummaryObj.GetComponent<TextMeshPro>();
            if (SummaryText != null)
            {
                SummaryText.autoSizeTextContainer = false;
                SummaryText.enableAutoSizing = false;
                SummaryText.lineSpacing = -25f;
                SummaryText.alignment = TextAlignmentOptions.TopLeft;
                SummaryText.color = Color.white;

                NetworkedPlayerInfo[] playersData = GameData.Instance.AllPlayers
                    .ToArray()
                    .OrderBy(pd => pd.Disconnected)  // Disconnected players last
                    .ThenBy(pd => pd.IsDead)
                    .ThenBy(pd => !pd.Role.IsImpostor)
                    .ToArray();        // Dead players after live players

                string winTeam;
                string winTag;
                string winColor;

                switch (EndGameResult.CachedGameOverReason)
                {
                    case GameOverReason.HumansByTask:
                        winTeam = "Cremates";
                        winTag = "Tasks Completion";
                        winColor = "#8cffff";
                        break;
                    case GameOverReason.HumansByVote:
                        winTeam = "Cremates";
                        winTag = "Imposters Voted Out";
                        winColor = "#8cffff";
                        break;
                    case GameOverReason.ImpostorDisconnect:
                        winTeam = "Cremates";
                        winTag = "Impostors Disconnected";
                        winColor = "#8cffff";
                        break;
                    case GameOverReason.ImpostorByKill:
                        winTeam = "Imposters";
                        winTag = "Crew Outnumbered";
                        winColor = "#f00202";
                        break;
                    case GameOverReason.ImpostorBySabotage:
                        winTeam = "Imposters";
                        winTag = "Sabotage";
                        winColor = "#f00202";
                        break;
                    case GameOverReason.ImpostorByVote:
                        winTeam = "Imposters";
                        winTag = "Crew Outnumbered";
                        winColor = "#f00202";
                        break;
                    case GameOverReason.HumansDisconnect:
                        winTeam = "Imposters";
                        winTag = "Cremates Disconnected";
                        winColor = "#f00202";
                        break;

                    // H&S

                    case GameOverReason.HideAndSeek_ByTimer:
                        winTeam = "Hiders";
                        winTag = "Time Out";
                        winColor = "#8cffff";
                        break;
                    case GameOverReason.HideAndSeek_ByKills:
                        winTeam = "Seekers";
                        winTag = "No Survivors";
                        winColor = "#f00202";
                        break;


                    default:
                        winTeam = "Unknown";
                        winTag = "Unknown";
                        winColor = "#ffffff";
                        break;
                }

                Logger.Log($"{winTeam}: {winTag}", "GameSummary");

                string SummaryHeader = "<align=\"center\"><size=150%>   Game Summary</size></align>";
                SummaryHeader += $"\n\n<size=90%><color={winColor}>{winTeam} Won</color></size>" +
                    $"\n<size=60%>\nBy {winTag}</size>";

                StringBuilder sb = new StringBuilder();

                foreach (var data in playersData)
                {
                    var name = $"<color={Utils.Color32ToHex(Palette.PlayerColors[data.DefaultOutfit.ColorId])}>{data.BetterData().RealName}</color>";
                    string playerTheme(string text) => $"<color={Utils.GetTeamHexColor(data.Role.TeamType)}>{text}</color>";

                    string roleInfo;
                    if (data.Role.IsImpostor)
                    {
                        roleInfo = $"({playerTheme(Utils.GetRoleName(data.RoleType))}) → {playerTheme($"Kills: {data.BetterData().RoleInfo.Kills}")}";
                    }
                    else
                    {
                        roleInfo = $"({playerTheme(Utils.GetRoleName(data.RoleType))}) → {playerTheme($"Tasks: {data.Tasks.ToArray().Where(task => task.Complete).Count()}/{data.Tasks.Count}")}";
                    }

                    string deathReason;
                    if (data.Disconnected)
                    {
                        deathReason = "『<color=#838383><b>D/C</b></color>』";
                    }
                    else if (!data.IsDead)
                    {
                        deathReason = "『<color=#80ff00><b>Alive</b></color>』";
                        ;
                    }
                    else if (data.IsDead)
                    {
                        deathReason = "『<color=#ff0600><b>Dead</b></color>』";
                    }
                    else
                    {
                        deathReason = "『<color=#838383<b>Unknown</b></color>』";
                    }

                    Logger.Log($"{name} {roleInfo} {deathReason}", "GameSummary");

                    sb.AppendLine($"- {name} {roleInfo} {deathReason}\n");
                }

                SummaryText.text = $"{SummaryHeader}\n\n<size=58%>{sb}</size>";
                Logger.LogHeader($"Game Summary End", "GameSummary");
            }
        }
        [HarmonyPatch(nameof(EndGameManager.ShowButtons))]
        [HarmonyPrefix]
        private static bool ShowButtons_Prefix(EndGameManager __instance)
        {
            __instance.FrontMost.gameObject.SetActive(false);
            __instance.Navigation.ShowDefaultNavigation();
            return false;
        }
    }
}
