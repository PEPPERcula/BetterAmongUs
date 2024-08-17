using BetterAmongUs.Patches;
using HarmonyLib;
using InnerNet;

namespace BetterAmongUs;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
class OnGameJoinedPatch
{
    public static void Postfix(/*AmongUsClient __instance*/)
    {
        try
        {
            PlayerControlExtensions.playerInfo.Clear();

            PlayerControlPatch.infotime = 0f;

            AntiCheat.PauseAntiCheat();

            // Fix host icon in lobby on modded servers
            if (!GameStates.IsVanillaServer)
            {
                var host = AmongUsClient.Instance.GetHost().Character;
                host.SetColor(-2);
                host.SetColor(host.CurrentOutfit.ColorId);
            }

            Logger.Log($"Successfully joined {GameCode.IntToGameName(AmongUsClient.Instance.GameId)}", "OnGameJoinedPatch");
        }
        catch { };
    }
}
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
public static class OnPlayerJoinedPatch
{
    public static void Postfix(/*AmongUsClient __instance,*/ [HarmonyArgument(0)] ClientData client)
    {
        PlayerControlPatch.infotime = 0f;

        _ = new LateTask(() =>
        {
            if (GameStates.IsInGame)
            {
                // Send Better Among Us Check RPC
                RPC.SendBetterCheck();

                RPC.SyncAllNames(force: true);

                if (Main.BetterHost.Value)
                    client.Character.RpcSendHostChat(HudManagerPatch.WelcomeMessage, sendToBetterUser: false);

                // Auto ban player on ban list
                var player = Utils.PlayerFromClientId(client.Id);
                if (player != null)
                {
                    string banPlayerListContent = File.ReadAllText(BetterDataManager.banPlayerListFile);

                    string[] listPlayerArray = banPlayerListContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                    foreach (string text in listPlayerArray)
                    {
                        if (!string.IsNullOrEmpty(player.Data.FriendCode) && text.Contains(player.Data.FriendCode)
                            || !string.IsNullOrEmpty(Utils.GetHashPuid(player)) && text.Contains(Utils.GetHashPuid(player)))
                        {
                            player.Kick(true, $"{player.Data.PlayerName} has been banned due to being on the ban player list!");
                            break;
                        }
                    }

                    // Normalize and remove spaces and special characters from names
                    Func<string, string> normalizeName = name => new string(name.Where(c => char.IsLetterOrDigit(c)).ToArray()).ToLower();

                    // Read all banned names into a HashSet with normalized names
                    HashSet<string> bannedNames = new HashSet<string>(
                        File.ReadLines(BetterDataManager.banNameListFile)
                            .Select(normalizeName)
                            .Where(name => !string.IsNullOrWhiteSpace(name))
                    );

                    string normalizedPlayerName = normalizeName(player.Data.PlayerName);

                    // Check if any banned name is a prefix of the player's normalized name
                    bool isNameBanned = bannedNames.Any(bannedName =>
                        normalizedPlayerName.StartsWith(bannedName)
                    );

                    if (!string.IsNullOrEmpty(normalizedPlayerName) && isNameBanned)
                    {
                        player.Kick(false, $"{player.Data.PlayerName} has been kicked due to their name being on the ban name list!");
                    }

                }
            }
        }, 2.5f, "OnPlayerJoinedPatch", false);
    }
}
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
class OnPlayerLeftPatch
{
    public static void Postfix(/*AmongUsClient __instance,*/ [HarmonyArgument(0)] ClientData data, [HarmonyArgument(1)] DisconnectReasons reason)
    {
        if ((GameStates.IsInGame || GameStates.IsFreePlay) && !GameStates.IsLobby && !GameStates.IsHideNSeek && HudManager.Instance.CrewmatesKilled.isActiveAndEnabled)
            HudManager.Instance?.NotifyOfDisconnect(data.Character);
    }
}

[HarmonyPatch(typeof(GameData))]
[HarmonyPatch("HandleDisconnect")]
[HarmonyPatch(MethodType.Normal)]
[HarmonyPatch(new Type[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
class GameDataHandleDisconnectPatch
{
    public static void Prefix(/*GameData __instance,*/ [HarmonyArgument(0)] PlayerControl player, [HarmonyArgument(1)] DisconnectReasons reason)
    {
        player.BetterData().DisconnectReason = reason;
    }
}

[HarmonyPatch(typeof(GameData), nameof(GameData.ShowNotification))]
class GameDataShowNotificationPatch
{
    public static bool Prefix(/*GameData __instance,*/ ref string playerName, ref DisconnectReasons reason)
    {
        string ReasonText;
        switch (reason)
        {
            case DisconnectReasons.ExitGame:
                ReasonText = $"{playerName} Left the game!";
                break;
            case DisconnectReasons.ClientTimeout:
                ReasonText = $"{playerName} Disconnected!";
                break;
            case DisconnectReasons.Kicked:
                ReasonText = $"{playerName} Was kicked by {AmongUsClient.Instance.GetHost().Character.Data.PlayerName}!";
                break;
            case DisconnectReasons.Banned:
                ReasonText = $"{playerName} Was banned by {AmongUsClient.Instance.GetHost().Character.Data.PlayerName}!";
                break;
            case DisconnectReasons.Hacking:
                ReasonText = $"{playerName} Was banned by Innersloth Anti-Cheat!";
                break;
            case DisconnectReasons.Error:
                ReasonText = $"{playerName} Was kicked due to an error!";
                break;
            case DisconnectReasons.Unknown:
                ReasonText = $"{playerName} Left the game due to unknown reason?";
                break;
            default:
                ReasonText = $"{playerName} Left the game!";
                break;
        }

        Logger.Log(ReasonText);

        DestroyableSingleton<HudManager>.Instance.Notifier.AddDisconnectMessage(ReasonText);
        return false;
    }
}
