using BetterAmongUs.Data;
using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using HarmonyLib;
using InnerNet;

namespace BetterAmongUs.Patches.Gameplay.Player;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
internal static class OnGameJoinedPatch
{
    private static void Postfix(/*AmongUsClient __instance*/)
    {
        // Fix host icon in lobby on modded servers
        if (!GameState.IsVanillaServer)
        {
            var host = AmongUsClient.Instance.GetHost().Character;
            host?.SetColor(-2);
            host?.SetColor(host.CurrentOutfit.ColorId);
        }

        Logger.Log($"Successfully joined {GameCode.IntToGameName(AmongUsClient.Instance.GameId)}", "OnGameJoinedPatch");
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
internal static class OnPlayerJoinedPatch
{
    private static void Postfix(/*AmongUsClient __instance,*/ [HarmonyArgument(0)] ClientData client)
    {
        _ = new LateTask(() =>
        {
            if (GameState.IsInGame)
            {
                var player = Utils.PlayerFromClientId(client.Id);

                // Auto ban player on ban list
                if (BetterGameSettings.UseBanPlayerList.GetBool())
                {
                    if (player != null)
                    {
                        if (TextFileHandler.CompareStringMatch(BetterDataManager.banPlayerListFile,
                            BAUPlugin.AllPlayerControls.Select(player => player.Data.FriendCode)
                            .Concat(BAUPlugin.AllPlayerControls.Select(player => player.GetHashPuid())).ToArray()))
                        {
                            player.Kick(true, Translator.GetString("AntiCheat.BanPlayerListMessage"), bypassDataCheck: true);
                        }
                    }
                }

                if (BetterGameSettings.UseBanNameList.GetBool())
                {
                    if (TextFileHandler.CompareStringFilters(BetterDataManager.banNameListFile, [player.Data.PlayerName]))
                    {
                        player.Kick(true, Translator.GetString("AntiCheat.BanPlayerListMessage"), bypassDataCheck: true);
                    }
                }
            }
        }, 2.5f, "OnPlayerJoinedPatch", false);
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
internal static class OnPlayerLeftPatch
{
    private static void Postfix(/*AmongUsClient __instance,*/ [HarmonyArgument(0)] ClientData data, [HarmonyArgument(1)] DisconnectReasons reason)
    {
    }
}

[HarmonyPatch(typeof(GameData))]
[HarmonyPatch("HandleDisconnect")]
[HarmonyPatch(MethodType.Normal)]
[HarmonyPatch(new Type[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
internal static class GameDataHandleDisconnectPatch
{
    private static void Prefix(/*GameData __instance,*/ [HarmonyArgument(0)] PlayerControl player, [HarmonyArgument(1)] DisconnectReasons reason)
    {
        if (player.BetterData() != null)
        {
            player.BetterData().DisconnectReason = reason;
        }

        GameDataShowNotificationPatch.BetterShowNotification(player.Data, reason);
    }
}

[HarmonyPatch(typeof(GameData), nameof(GameData.ShowNotification))]
internal static class GameDataShowNotificationPatch
{
    internal static void BetterShowNotification(NetworkedPlayerInfo playerData, DisconnectReasons reason = DisconnectReasons.Unknown, string forceReasonText = "")
    {
        if (playerData.BetterData().AntiCheatInfo.BannedByAntiCheat || playerData.BetterData().HasShowDcMsg) return;
        playerData.BetterData().HasShowDcMsg = true;

        string? playerName = playerData.BetterData().RealName;

        if (forceReasonText != "")
        {
            var ReasonText = $"<color=#ff0>{playerData.BetterData().RealName}</color> {forceReasonText}";

            Logger.Log(ReasonText);

            HudManager.Instance.Notifier.AddDisconnectMessage(ReasonText);
        }
        else
        {
            string ReasonText;

            switch (reason)
            {
                case DisconnectReasons.ExitGame:
                    ReasonText = string.Format(Translator.GetString("DisconnectReason.Left"), playerName);
                    break;
                case DisconnectReasons.ClientTimeout:
                    ReasonText = string.Format(Translator.GetString("DisconnectReason.Disconnect"), playerName);
                    break;
                case DisconnectReasons.Kicked:
                    ReasonText = string.Format(Translator.GetString("DisconnectReason.Kicked"), playerName, AmongUsClient.Instance.GetHost().Character.Data.PlayerName);
                    break;
                case DisconnectReasons.Banned:
                    ReasonText = string.Format(Translator.GetString("DisconnectReason.Banned"), playerName, AmongUsClient.Instance.GetHost().Character.Data.PlayerName);
                    break;
                case DisconnectReasons.Hacking:
                    ReasonText = string.Format(Translator.GetString("DisconnectReason.Cheater"), playerName);
                    break;
                case DisconnectReasons.Error:
                    ReasonText = string.Format(Translator.GetString("DisconnectReason.Error"), playerName);
                    break;
                case DisconnectReasons.Unknown:
                    ReasonText = string.Format(Translator.GetString("DisconnectReason.Unknown"), playerName);
                    break;
                default:
                    ReasonText = string.Format(Translator.GetString("DisconnectReason.Left"), playerName);
                    break;
            }

            Logger.Log(ReasonText);

            HudManager.Instance.Notifier.AddDisconnectMessage(ReasonText);
        }
    }

    internal static bool Prefix()
    {
        return false;
    }
}
