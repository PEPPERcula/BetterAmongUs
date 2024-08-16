using AmongUs.Data;
using HarmonyLib;
using Il2CppSystem.Linq;
using LibCpp2IL;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Patches;

[HarmonyPatch(typeof(PlayerControl))]
class PlayerControlPatch
{
    public static float infotime = 0f;
    [HarmonyPatch(nameof(PlayerControl.FixedUpdate))]
    [HarmonyPrefix]
    public static void FixedUpdate_Prefix(PlayerControl __instance)
    {
        // Set up player text info
        var nameTextTransform = __instance.gameObject.transform.Find("Names/NameText_TMP");
        var nameText = nameTextTransform?.gameObject;
        var infoText = nameTextTransform?.Find("InfoText_T_TMP");

        if (nameText != null && infoText == null)
        {
            void InstantiatePlayerInfoText(string name, Vector3 positionOffset)
            {
                var newTextObject = UnityEngine.Object.Instantiate(nameText, nameTextTransform);
                newTextObject.name = name;
                newTextObject.transform.DestroyChildren();
                newTextObject.transform.position += positionOffset;
                var textMesh = newTextObject.GetComponent<TextMeshPro>();
                if (textMesh != null)
                {
                    textMesh.text = string.Empty;
                }
                newTextObject.SetActive(true);
            }

            InstantiatePlayerInfoText("InfoText_Info_TMP", new Vector3(0f, 0.25f));
            InstantiatePlayerInfoText("InfoText_T_TMP", new Vector3(0f, 0.15f));
            InstantiatePlayerInfoText("InfoText_B_TMP", new Vector3(0f, -0.15f));
        }

        // Set color blind text on player
        if (__instance.DataIsCollected() && !__instance.shapeshifting)
        {
            __instance.cosmetics.SetColorBlindColor(__instance.CurrentOutfit.ColorId);
        }
        else
        {
            __instance.cosmetics.colorBlindText.text = string.Empty;
        }

        if (GameStates.IsInGame && GameStates.IsHost && Main.BetterHost.Value)
        {
            BetterHostManager.PlayerUpdate(__instance);
        }

        // Set text info
        infotime -= Time.deltaTime;

        if (infotime <= 0)
        {
            SetPlayerInfo(__instance);
            infotime = 0.4f;
        }

        // Reset player data in lobby
        if (!GameStates.IsInGamePlay)
        {
            __instance.BetterData().TimesCalledMeeting = 0;
            __instance.BetterData().HasNoisemakerNotify = false;
            __instance.BetterData().TimeSinceLastTask = 5f;
            __instance.BetterData().LastTaskId = 999;
        }
        else
        {
            __instance.BetterData().TimeSinceLastTask += Time.deltaTime;
        }
    }

    public static void SetPlayerInfo(PlayerControl player)
    {
        try
        {
            if (player == null || player.Data == null) return;

            var nameText = player.gameObject.transform.Find("Names/NameText_TMP").GetComponent<TextMeshPro>();

            // Set player text info
            if (!player.DataIsCollected())
            {
                nameText.text = "<color=#b5b5b5>Loading</color>";
                return;
            }

            if (!Main.LobbyPlayerInfo.Value && GameStates.IsLobby)
            {
                player.ResetAllPlayerTextInfo();
                player.RawSetName(player.Data.PlayerName);
                return;
            }

            if (Main.AllPlayerControls.Any(pc => player.Data.PlayerId == pc.shapeshiftTargetPlayerId))
                return;

            string NewName = player.Data.PlayerName;
            string hashPuid = Utils.GetHashPuid(player);

            string platform = Utils.GetPlatformName(player, useTag: true);

            string pattern = @"^[a-zA-Z0-9#]+$";
            string hashtagPattern = @"^#[0-9]{4}$";
            string friendCode = player.Data.FriendCode;
            string friendCodeColor = (Regex.Replace(friendCode, hashtagPattern, string.Empty).Length is > 10 or < 5
                || !Regex.IsMatch(friendCode, pattern)
                || !Regex.IsMatch(friendCode, hashtagPattern)
                || friendCode.Contains(' ')) ? "#00f7ff" : "#ff0000";
            if (string.IsNullOrEmpty(friendCode) || friendCode == "")
            {
                friendCode = "No Friend Code";
                friendCodeColor = "#ff0000";
            }
            if (DataManager.Settings.Gameplay.StreamerMode == true)
            {
                friendCode = string.Concat('*').Repeat(friendCode.Length);
                platform = "Platform Hidden";
            }

            var sbTag = new StringBuilder();
            var sbInfo = new StringBuilder();
            var sbTagTop = new StringBuilder();
            var sbInfoTop = new StringBuilder();
            var sbTagBottom = new StringBuilder();
            var sbInfoBottom = new StringBuilder();

            // Put +++ at the end of each tag
            if (!player.isDummy)
            {
                if (!string.IsNullOrEmpty(hashPuid) && AntiCheat.SickoData.ContainsKey(hashPuid) || !string.IsNullOrEmpty(friendCode) && AntiCheat.SickoData.ContainsValue(friendCode))
                    sbTag.Append("<color=#00f583>Sicko User</color>+++");
                else if (!string.IsNullOrEmpty(hashPuid) && AntiCheat.AUMData.ContainsKey(hashPuid) || !string.IsNullOrEmpty(friendCode) && AntiCheat.AUMData.ContainsValue(friendCode))
                    sbTag.Append("<color=#4f0000>AUM User</color>+++");
                else if (!string.IsNullOrEmpty(hashPuid) && AntiCheat.PlayerData.ContainsKey(hashPuid) || !string.IsNullOrEmpty(friendCode) && AntiCheat.PlayerData.ContainsValue(friendCode))
                    sbTag.Append("<color=#fc0000>Known Cheater</color>+++");
            }

            if (GameStates.IsInGame && GameStates.IsLobby && !GameStates.IsFreePlay)
            {
                if (player.IsHost())
                {
                    if (Main.LobbyPlayerInfo.Value == true)
                    {
                        NewName = player.GetPlayerNameAndColor();
                    }
                }

                if (player.IsDev() && !GameStates.IsInGamePlay)
                    sbTag.Append("<color=#6e6e6e>(<color=#0088ff>Dev</color>)</color>+++");
                if (((player == PlayerControl.LocalPlayer && GameStates.IsHost && Main.BetterHost.Value) || player.BetterData().IsBetterHost) && !GameStates.IsInGamePlay)
                    sbTag.Append("<color=#0dff00>Better Host</color>+++");
                else if ((player == PlayerControl.LocalPlayer || player.BetterData().IsBetterUser) && !GameStates.IsInGamePlay)
                    sbTag.Append("<color=#0dff00>Better User</color>+++");
                sbTag.Append($"<color=#b554ff>ID: {player.PlayerId}</color>+++");

                sbTagTop.Append($"<color=#9e9e9e>{platform}</color>+++");

                sbTagTop.Append($"<color=#ffd829>Lv: {player.Data.PlayerLevel.ToString()}</color>+++");

                sbTagBottom.Append($"<color={friendCodeColor}>{friendCode}</color>+++");
            }
            else if (GameStates.IsInGame || GameStates.IsFreePlay)
            {
                if (player.IsImpostorTeammate() || player == PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.IsAlive() || DebugMenu.RevealRoles)
                {
                    string Role = $"<color={player.GetTeamHexColor()}>{player.GetRoleName()}</color>";
                    if (!player.IsImpostorTeam() && player.myTasks.Count > 0)
                    {
                        Role += $" <color=#cbcbcb>({player.myTasks.ToArray().Where(task => task.IsComplete).Count()}/{player.myTasks.Count})</color>";
                    }
                    sbTagTop.Append($"{Role}+++");
                }
            }

            if (player.IsImpostorTeammate() && player != PlayerControl.LocalPlayer)
            {
                NewName = $"<color=#ff1919>{player.Data.PlayerName}</color>";
            }

            if (!player.IsInShapeshift())
            {
                player.RawSetName(NewName);
            }

            // Put +++ at the end of each tag
            void AppendTagInfo(StringBuilder source, StringBuilder destination)
            {
                if (source.Length > 0)
                {
                    string text = source.ToString();
                    string[] parts = text.Split("+++");
                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(Utils.GetRawText(parts[i])))
                        {
                            destination.Append(parts[i]);
                            if (i != parts.Length - 2)
                            {
                                destination.Append(" - ");
                            }
                        }
                    }
                }
            }

            AppendTagInfo(sbTag, sbInfo);
            AppendTagInfo(sbTagTop, sbInfoTop);
            AppendTagInfo(sbTagBottom, sbInfoBottom);

            player.SetPlayerTextInfo(sbInfoTop.ToString());
            player.SetPlayerTextInfo(sbInfoBottom.ToString(), isBottom: true);
            player.SetPlayerTextInfo(sbInfo.ToString(), isInfo: true);
        }
        catch (Exception ex)
        {
            Logger.Error(ex.ToString());
        }
    }


    [HarmonyPatch(nameof(PlayerControl.SetColor))]
    [HarmonyPostfix]
    public static void SetColor_Postfix()
    {
        if (Main.BetterHost.Value && GameStates.IsLobby)
        {
            _ = new LateTask(() =>
            {
                RPC.SyncAllNames(force: true);
            }, 0.25f, shoudLog: false);
        }
    }

    [HarmonyPatch(nameof(PlayerControl.MurderPlayer))]
    [HarmonyPostfix]
    public static void MurderPlayer_Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        if (target == null) return;

        if (PlayerControl.LocalPlayer.IsImpostorTeam() && GameStates.IsInGamePlay && !GameStates.IsHideNSeek && HudManager.Instance.CrewmatesKilled.isActiveAndEnabled)
            HudManager.Instance?.NotifyOfDeath();

        Logger.LogPrivate($"{__instance.Data.PlayerName} Has killed {target.Data.PlayerName} as {Utils.GetRoleName(__instance.Data.RoleType)}", "EventLog");
    }
    [HarmonyPatch(nameof(PlayerControl.Shapeshift))]
    [HarmonyPostfix]
    public static void Shapeshift_Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target, [HarmonyArgument(1)] bool animate)
    {
        if (target == null) return;

        if (__instance != target)
            Logger.LogPrivate($"{__instance.Data.PlayerName} Has Shapeshifted into {target.Data.PlayerName}, did animate: {animate}", "EventLog");
        else
            Logger.LogPrivate($"{__instance.Data.PlayerName} Has Un-Shapeshifted, did animate: {animate}", "EventLog");
    }
    [HarmonyPatch(nameof(PlayerControl.SetRoleInvisibility))]
    [HarmonyPostfix]
    public static void SetRoleInvisibility_Postfix(PlayerControl __instance, [HarmonyArgument(0)] bool isActive, [HarmonyArgument(1)] bool animate)
    {

        if (isActive)
            Logger.LogPrivate($"{__instance.Data.PlayerName} Has Vanished as Phantom, did animate: {animate}", "EventLog");
        else
            Logger.LogPrivate($"{__instance.Data.PlayerName} Has Appeared as Phantom, did animate: {animate}", "EventLog");
    }
}

[HarmonyPatch(typeof(PlayerPhysics))]
public class PlayerPhysicsPatch
{
    [HarmonyPatch(nameof(PlayerPhysics.BootFromVent))]
    [HarmonyPostfix]
    private static void BootFromVent_Postfix(PlayerPhysics __instance, [HarmonyArgument(0)] int ventId)
    {

        Logger.LogPrivate($"{__instance.myPlayer.Data.PlayerName} Has been booted from vent: {ventId}, as {Utils.GetRoleName(__instance.myPlayer.Data.RoleType)}", "EventLog");
    }
    [HarmonyPatch(nameof(PlayerPhysics.CoEnterVent))]
    [HarmonyPostfix]
    private static void CoEnterVent_Postfix(PlayerPhysics __instance, [HarmonyArgument(0)] int ventId)
    {

        Logger.LogPrivate($"{__instance.myPlayer.Data.PlayerName} Has entered vent: {ventId}, as {Utils.GetRoleName(__instance.myPlayer.Data.RoleType)}", "EventLog");
    }
    [HarmonyPatch(nameof(PlayerPhysics.CoExitVent))]
    [HarmonyPostfix]
    private static void CoExitVent_Postfix(PlayerPhysics __instance, [HarmonyArgument(0)] int ventId)
    {

        Logger.LogPrivate($"{__instance.myPlayer.Data.PlayerName} Has exit vent: {ventId}, as {Utils.GetRoleName(__instance.myPlayer.Data.RoleType)}", "EventLog");
    }
}
