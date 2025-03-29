using AmongUs.Data;
using AmongUs.GameOptions;
using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using BetterAmongUs.Modules.AntiCheat;
using HarmonyLib;
using Il2CppSystem.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;

namespace BetterAmongUs.Patches;

[HarmonyPatch(typeof(PlayerControl))]
class PlayerControlPatch
{
    [HarmonyPatch(nameof(PlayerControl.Start))]
    [HarmonyPostfix]
    internal static void Start_Postfix(PlayerControl __instance)
    {
        Main.AllPlayerControls.Add(__instance);
    }

    [HarmonyPatch(nameof(PlayerControl.OnDestroy))]
    [HarmonyPostfix]
    internal static void OnDestroy_Postfix(PlayerControl __instance)
    {
        Main.AllPlayerControls.Remove(__instance);
    }

    [HarmonyPatch(nameof(PlayerControl.FixedUpdate))]
    [HarmonyPrefix]
    internal static void FixedUpdate_Prefix(PlayerControl __instance)
    {
        SetPlayerInfo(__instance);
        SetPlayerHighlight(__instance);
        __instance.UpdateColorBlindTextPosition();
    }

    internal static void SetPlayerHighlight(PlayerControl player)
    {
        string hashPuid = Utils.GetHashPuid(player);
        string friendCode = player.Data.FriendCode;

        SetPlayerOutline(player, hashPuid, friendCode, new());
    }

    internal static void SetPlayerInfo(PlayerControl player)
    {
        if (player?.Data == null || player?.BetterData()?.IsDirtyInfo != true) return;
        player.BetterData().IsDirtyInfo = false;

        var betterData = player.BetterData();
        if (GameState.IsTOHEHostLobby) return;

        var nameText = player.gameObject.transform.Find("Names/NameText_TMP").GetComponent<TextMeshPro>();

        if (!player.DataIsCollected())
        {
            nameText.text = Translator.GetString("Player.Loading");
            return;
        }

        if (!Main.LobbyPlayerInfo.Value && GameState.IsLobby)
        {
            player.ResetAllPlayerTextInfo();
            player.RawSetName(player.Data.PlayerName);
            return;
        }

        string newName = player.Data.PlayerName;
        string hashPuid = Utils.GetHashPuid(player);
        string platform = Utils.GetPlatformName(player, useTag: true);

        string friendCode = ValidateFriendCode(player, out string friendCodeColor);

        if (DataManager.Settings.Gameplay.StreamerMode)
        {
            platform = Translator.GetString("Player.PlatformHidden");
        }

        // Set Tags
        var sbTag = new StringBuilder();
        var sbTagTop = new StringBuilder();
        var sbTagBottom = new StringBuilder();

        SetPlayerOutline(player, hashPuid, player.Data.FriendCode, sbTag);

        if (GameState.IsInGame && GameState.IsLobby && !GameState.IsFreePlay)
        {
            SetLobbyInfo(player, ref newName, betterData, sbTag);
            sbTagTop.Append($"<color=#9e9e9e>{platform}</color>+++")
                    .Append($"<color=#ffd829>Lv: {player.Data.PlayerLevel + 1}</color>+++");

            sbTagBottom.Append($"<color={friendCodeColor}>{friendCode}</color>+++");
        }
        else if ((GameState.IsInGame || GameState.IsFreePlay) && !GameState.IsHideNSeek)
        {
            SetInGameInfo(player, sbTagTop);
        }

        if (!player.IsInShapeshift())
        {
            player.RawSetName(newName);
        }
        else
        {
            var targetData = Utils.PlayerDataFromPlayerId(player.shapeshiftTargetPlayerId);
            if (targetData != null) player.RawSetName(targetData.BetterData().RealName);
        }

        player.SetPlayerTextInfo(FormatInfo(sbTagTop));
        player.SetPlayerTextInfo(FormatInfo(sbTagBottom), isBottom: true);
        player.SetPlayerTextInfo(FormatInfo(sbTag), isInfo: true);
    }

    private static string ValidateFriendCode(PlayerControl player, out string color)
    {
        color = "#FFFFFF";
        if (player?.Data == null) return string.Empty;

        void TryKick()
        {
            if (GameState.IsHost)
            {
                if (BetterGameSettings.InvalidFriendCode.GetBool())
                {
                    string kickMessage = string.Format(Translator.GetString("AntiCheat.KickMessage"), Translator.GetString("AntiCheat.ByAntiCheat"), Translator.GetString("AntiCheat.Reason.InvalidFriendCode"));
                    player.Kick(true, kickMessage, true);
                }
            }
        }

        string pattern = @"^[a-zA-Z0-9#]+$";
        string hashtagPattern = @"^#[0-9]{4}$";
        string friendCode = player.Data.FriendCode;
        color = (Regex.Replace(friendCode, hashtagPattern, string.Empty).Length is > 10 or < 5
            || !Regex.IsMatch(friendCode, pattern)
            || !Regex.IsMatch(friendCode, hashtagPattern)
            || friendCode.Contains(' ')) ? "#00f7ff" : "#ff0000";

        if (string.IsNullOrEmpty(friendCode))
        {
            friendCode = Translator.GetString("Player.NoFriendCode");
            color = "#ff0000";

            TryKick();

            return friendCode;
        }
        else if (color == "#ff0000")
        {
            TryKick();
        }

        if (DataManager.Settings.Gameplay.StreamerMode)
        {
            friendCode = new string('*', friendCode.Length);
        }

        return friendCode.Trim();
    }

    private static void SetPlayerOutline(PlayerControl player, string hashPuid, string friendCode, StringBuilder sbTag)
    {
        var color = player.cosmetics.currentBodySprite.BodySprite.material.GetColor("_OutlineColor");
        if ((!string.IsNullOrEmpty(hashPuid) && hashPuid.Length > 0 && BetterAntiCheat.SickoData.ContainsKey(hashPuid))
            || (!string.IsNullOrEmpty(friendCode) && friendCode.Length > 0 && BetterAntiCheat.SickoData.ContainsValue(friendCode)))
        {
            sbTag.Append($"<color=#00f583>{Translator.GetString("Player.SickoUser")}</color>+++");
            player.SetOutlineByHex(true, "#00f583");
        }
        else if ((!string.IsNullOrEmpty(hashPuid) && hashPuid.Length > 0 && BetterAntiCheat.AUMData.ContainsKey(hashPuid))
            || (!string.IsNullOrEmpty(friendCode) && friendCode.Length > 0 && BetterAntiCheat.AUMData.ContainsValue(friendCode)))
        {
            sbTag.Append($"<color=#4f0000>{Translator.GetString("Player.AUMUser")}</color>+++");
            player.SetOutlineByHex(true, "#4f0000");
        }
        else if ((!string.IsNullOrEmpty(hashPuid) && hashPuid.Length > 0 && BetterAntiCheat.KNData.ContainsKey(hashPuid))
            || (!string.IsNullOrEmpty(friendCode) && friendCode.Length > 0 && BetterAntiCheat.KNData.ContainsValue(friendCode)))
        {
            sbTag.Append($"<color=#8731e7>{Translator.GetString("Player.KNUser")}</color>+++");
            player.SetOutlineByHex(true, "#8731e7");
        }
        else if ((!string.IsNullOrEmpty(hashPuid) && hashPuid.Length > 0 && BetterAntiCheat.PlayerData.ContainsKey(hashPuid))
            || (!string.IsNullOrEmpty(friendCode) && friendCode.Length > 0 && BetterAntiCheat.PlayerData.ContainsValue(friendCode)))
        {
            sbTag.Append($"<color=#fc0000>{Translator.GetString("Player.KnownCheater")}</color>+++");
            player.SetOutlineByHex(true, "#fc0000");
        }
        else if (color == Utils.HexToColor32("#00f583") || color == Utils.HexToColor32("#4f0000") || color == Utils.HexToColor32("#fc0000") || color == Utils.HexToColor32("#8731e7"))
        {
            player.SetOutline(false, null);
        }
    }

    private static void SetLobbyInfo(PlayerControl player, ref string newName, ExtendedPlayerInfo betterData, StringBuilder sbTag)
    {
        if (player.IsHost() && Main.LobbyPlayerInfo.Value)
            newName = player.GetPlayerNameAndColor();

        if ((player.IsLocalPlayer() || betterData.IsBetterUser) && !GameState.IsInGamePlay)
        {
            sbTag.AppendFormat("<color=#0dff00>{1}{0}</color>+++", Translator.GetString("Player.BetterUser"),
                betterData.IsVerifiedBetterUser || player.IsLocalPlayer() ? "✓ " : "");
        }
        sbTag.Append($"<color=#b554ff>ID: {player.PlayerId}</color>+++");
    }

    private static void SetInGameInfo(PlayerControl player, StringBuilder sbTagTop)
    {
        if (player.IsImpostorTeammate() || player.IsLocalPlayer() || (!PlayerControl.LocalPlayer.IsAlive() && !PlayerControl.LocalPlayer.Is(RoleTypes.GuardianAngel)))
        {
            string roleInfo = $"<color={player.GetTeamHexColor()}>{player.GetRoleName()}</color>";
            if (!player.IsImpostorTeam() && player.myTasks.Count > 0)
            {
                roleInfo += $" <color=#cbcbcb>({player.Data.Tasks.ToArray().Where(task => task.Complete).Count()}/{player.Data.Tasks.Count})</color>";
            }
            sbTagTop.Append(roleInfo + "+++");
        }
    }

    private static string FormatInfo(StringBuilder source)
    {
        if (source.Length == 0) return string.Empty;

        var sb = new StringBuilder();
        foreach (var part in source.ToString().Split("+++"))
        {
            if (!string.IsNullOrEmpty(Utils.RemoveHtmlText(part)))
            {
                sb.Append(part).Append(" - ");
            }
        }
        return sb.ToString().TrimEnd(" - ".ToCharArray());
    }


    [HarmonyPatch(nameof(PlayerControl.RpcSetName))]
    [HarmonyPrefix]
    internal static bool RpcSetName_Prefix(PlayerControl __instance, [HarmonyArgument(0)] string name)
    {

        Utils.DirtyAllNames();

        return true;
    }

    [HarmonyPatch(nameof(PlayerControl.CompleteTask))]
    [HarmonyPostfix]
    internal static void CompleteTask_Postfix(PlayerControl __instance)
    {
        __instance.DirtyName();
    }

    [HarmonyPatch(nameof(PlayerControl.CoSetRole))]
    [HarmonyPostfix]
    internal static void CoSetRole_Postfix(PlayerControl __instance)
    {
        __instance.DirtyName();
    }

    [HarmonyPatch(nameof(PlayerControl.Revive))]
    [HarmonyPostfix]
    internal static void Revive_Postfix(PlayerControl __instance)
    {
        Utils.DirtyAllNames();
    }

    [HarmonyPatch(nameof(PlayerControl.MurderPlayer))]
    [HarmonyPostfix]
    internal static void MurderPlayer_Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        if (target == null) return;

        Logger.LogPrivate($"{__instance.Data.PlayerName} Has killed {target.Data.PlayerName} as {Utils.GetRoleName(__instance.Data.RoleType)}", "EventLog");

        __instance.BetterData().RoleInfo.Kills += 1;

        Utils.DirtyAllNames();
    }
    [HarmonyPatch(nameof(PlayerControl.Shapeshift))]
    [HarmonyPostfix]
    internal static void Shapeshift_Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target, [HarmonyArgument(1)] bool animate)
    {
        if (target == null) return;

        if (__instance != target)
            Logger.LogPrivate($"{__instance.Data.PlayerName} Has Shapeshifted into {target.Data.PlayerName}, did animate: {animate}", "EventLog");
        else
            Logger.LogPrivate($"{__instance.Data.PlayerName} Has Un-Shapeshifted, did animate: {animate}", "EventLog");
    }
    [HarmonyPatch(nameof(PlayerControl.SetRoleInvisibility))]
    [HarmonyPostfix]
    internal static void SetRoleInvisibility_Postfix(PlayerControl __instance, [HarmonyArgument(0)] bool isActive, [HarmonyArgument(1)] bool animate)
    {

        if (isActive)
            Logger.LogPrivate($"{__instance.Data.PlayerName} Has Vanished as Phantom, did animate: {animate}", "EventLog");
        else
            Logger.LogPrivate($"{__instance.Data.PlayerName} Has Appeared as Phantom, did animate: {animate}", "EventLog");
    }
}

[HarmonyPatch(typeof(PlayerPhysics))]
internal class PlayerPhysicsPatch
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
