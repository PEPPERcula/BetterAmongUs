using HarmonyLib;
using Newtonsoft.Json.Utilities;
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
        }
    }

    [HarmonyPatch(typeof(GameManager))]
    public class GameManagerPatch
    {
        [HarmonyPatch(nameof(GameManager.EndGame))]
        [HarmonyPostfix]
        private static void Postfix(/*GameManager __instance*/)
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
        [HarmonyPatch(nameof(GameStartManager.Update))]
        [HarmonyPrefix]
        private static void Update_Prefix(GameStartManager __instance)
        {
            __instance.MinPlayers = 1;
        }
        [HarmonyPatch(nameof(GameStartManager.Update))]
        [HarmonyPostfix]
        private static void Update_Postfix(GameStartManager __instance)
        {
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

            return true;
        }
    }
    [HarmonyPatch(typeof(EndGameManager))]
    public class EndGameManagerPatch
    {
        [HarmonyPatch(nameof(EndGameManager.SetEverythingUp))]
        [HarmonyPostfix]
        private static void SetEverythingUp_Postfix(EndGameManager __instance)
        {
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

                NetworkedPlayerInfo[] playersData = GameData.Instance.AllPlayers.ToArray();

                string SummaryHeader = "<align=\"center\">Game Summary</align>";

                StringBuilder sb = new StringBuilder();

                foreach (var data in playersData)
                {
                    var name = $"<color={Utils.Color32ToHex(Palette.PlayerColors[data.DefaultOutfit.ColorId])}>{data.PlayerName}</color>";
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

                    sb.AppendLine($"- {deathReason} {name} {roleInfo}\n");
                }

                SummaryText.text = $"{SummaryHeader}\n\n<size=58%>{sb}</size>";
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
