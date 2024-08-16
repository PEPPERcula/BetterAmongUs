using BetterAmongUs.Patches;
using HarmonyLib;
using Hazel;
using InnerNet;
using UnityEngine.ProBuilder;

namespace BetterAmongUs;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
class OnGameJoinedPatch
{
    public static void Postfix(/*AmongUsClient __instance*/)
    {
        try
        {
            PlayerControlPatch.infotime = 0f;

            AntiCheat.PauseAntiCheat();

            // Fix host icon in lobby on modded servers
            if (!GameStates.IsVanillaServer)
            {
                var host = AmongUsClient.Instance.GetHost().Character;
                host.SetColor(-2);
                host.SetColor(host.CurrentOutfit.ColorId);
            }

            _ = new LateTask(() =>
            {
                // Send Better Among Us Check RPC
                if (GameStates.IsInGame)
                {
                    var flag = GameStates.IsHost && Main.BetterHost.Value;
                    MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.BetterCheck, SendOption.None, -1);
                    messageWriter.Write((byte)PlayerControl.LocalPlayer.NetId);
                    messageWriter.Write(flag);
                    messageWriter.Write(Main.GetVersionText().Replace(" ", ""));
                    AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
                }
            }, 1.5f, "OnGameJoinedPatch");

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
                var flag = GameStates.IsHost && Main.BetterHost.Value;
                MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, unchecked((byte)CustomRPC.BetterCheck), SendOption.None, -1);
                messageWriter.Write((byte)PlayerControl.LocalPlayer.NetId);
                messageWriter.Write(flag);
                messageWriter.Write(Main.GetVersionText().Replace(" ", ""));
                AmongUsClient.Instance.FinishRpcImmediately(messageWriter);

                RPC.SyncAllNames(force: true);

                if (Main.BetterHost.Value)
                    client.Character.RpcSendHostChat(HudManagerPatch.WelcomeMessage, sendToBetterUser: false);

                // Auto ban player on ban list
                var player = Utils.PlayerFromClientId(client.Id);
                if (player != null)
                {
                    string banListContent = File.ReadAllText(BetterDataManager.banListFile);

                    string[] listArray = banListContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                    foreach (string text in listArray)
                    {
                        if (!string.IsNullOrEmpty(player.Data.FriendCode) && text.Contains(player.Data.FriendCode)
                            || !string.IsNullOrEmpty(Utils.GetHashPuid(player)) && text.Contains(Utils.GetHashPuid(player)))
                        {
                            player.Kick(true, $"{player.Data.PlayerName} has been banned due to being on the ban list!");
                            break;
                        }
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

[HarmonyPatch(typeof(GameData), nameof(GameData.ShowNotification))]
class GameDataShowNotificationPatch
{
    public static bool Prefix(AmongUsClient __instance, ref string playerName, ref DisconnectReasons reason)
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
