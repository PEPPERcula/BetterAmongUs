using BetterAmongUs.Patches;
using HarmonyLib;
using Hazel;
using static UnityEngine.GraphicsBuffer;

namespace BetterAmongUs;

enum CustomRPC : int
{
    // Cheat RPC's
    Sicko = 420,
    AUM = 42069,
    AUMChat = 101,

    // TOHE
    VersionCheck = 80,
    RequestRetryVersionCheck = 81,

    //Better Among Us
    BetterCheck = 150,
    AddChat,
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
internal class RPCHandlerPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        AntiCheat.HandleRPCBeforeCheck(__instance, callId, reader);

        if (AntiCheat.CheckCancelRPC(__instance, callId, reader) != true)
        {
            Logger.LogCheat($"RPC canceled by Anti-Cheat: {Enum.GetName((RpcCalls)callId)}{Enum.GetName((CustomRPC)callId)} - {callId}");
            return false;
        }

        if (GameStates.IsHost && BetterHostManager.CheckRPCAsHost(__instance, callId, reader) != true)
        {
            Logger.LogCheat($"RPC canceled by Host: {Enum.GetName((RpcCalls)callId)}{Enum.GetName((CustomRPC)callId)} - {callId}");
            return false;
        }

        return true;
    }
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        AntiCheat.CheckRPC(__instance, callId, reader);
        RPC.HandleRPC(__instance, callId, reader);
    }
}

internal static class RPC
{
    public static void SetNamePrivate(PlayerControl player, string name, PlayerControl target)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetName, SendOption.Reliable, target.GetClientId());
        writer.Write(player.NetId);
        writer.Write(name);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    public static void SyncAllNames(bool isForMeeting = false, bool force = false, bool isBetterHost = true)
    {
        if (!GameStates.IsHost) return;

        foreach (PlayerControl player in Main.AllPlayerControls)
            BetterHostManager.SetPlayersInfoAsHost(player, isForMeeting, force, isBetterHost);
    }

    public static void ExileAsync(PlayerControl player)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.Exiled, SendOption.Reliable, -1);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        player.Exiled();
    }

    public static async Task WaitForChatCooldown()
    {
        while (3f - HudManager.Instance.Chat.timeSinceLastMessage > 0f)
        {
            await Task.Delay(100);
        }
    }

    public static async void SendHostChatToPlayer(PlayerControl player, string text, string? title = "", bool sendToBetterUser = true)
    {
        if (!GameStates.IsHost) return;

        if (player.GetIsBetterUser())
        {
            if (sendToBetterUser)
            {
                MessageWriter messageWriter2 = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, unchecked((byte)CustomRPC.AddChat), SendOption.None, player.GetClientId());
                messageWriter2.Write(text);
                messageWriter2.Write(title);
                AmongUsClient.Instance.FinishRpcImmediately(messageWriter2);
            }

            return;
        }

        return;

        await WaitForChatCooldown();

        PlayerControl asPlayer = Main.AllPlayerControls.Where(pc => pc.IsAlive()).OrderBy(pc => pc == PlayerControl.LocalPlayer ? 0 : 1).First();

        var oldName = asPlayer.CurrentOutfit.PlayerName;
        if (title == "")
            title = oldName;

        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(asPlayer.NetId, (byte)RpcCalls.SetName, SendOption.None, player.GetClientId());
        writer.Write(asPlayer.Data.NetId);
        writer.Write(title);
        AmongUsClient.Instance.FinishRpcImmediately(writer);

        MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(asPlayer.NetId, (byte)RpcCalls.SendChat, SendOption.None, player.GetClientId());
        messageWriter.Write(text);
        AmongUsClient.Instance.FinishRpcImmediately(messageWriter);

        MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(asPlayer.NetId, (byte)RpcCalls.SetName, SendOption.None, player.GetClientId());
        writer2.Write(asPlayer.Data.NetId);
        writer2.Write(oldName);
        AmongUsClient.Instance.FinishRpcImmediately(writer2);

        SyncAllNames();
    }

    public static void HandleRPC(PlayerControl player, byte callId, MessageReader reader)
    {
        if (PlayerControl.LocalPlayer == null || player == null || player == PlayerControl.LocalPlayer || player.Data == null) return;

        switch (callId)
        {
            case (byte)CustomRPC.BetterCheck:
                if (reader.ReadByte() == player.NetId)
                    player.SetIsBetterUser(true);
                break;
            case (byte)CustomRPC.AddChat:
                Utils.AddChatPrivate(reader.ReadString(), reader.ReadString());
                break;
            case (byte)CustomRPC.VersionCheck:
                if (player.IsHost())
                {
                    var BAU = "<color=#278720>♻</color><color=#0ed400><b>BetterAmongUs</b></color><color=#278720>♻</color>";
                    Utils.DisconnectSelf($"{BAU} does not support <color=#ff9cdc><b>TOHE</b></color>");
                }
                break;
            case (byte)RpcCalls.SendChat:
                if (player.IsHost() && player != PlayerControl.LocalPlayer)
                {
                    if (reader.BytesRemaining > 0)
                    {
                        if (reader.ReadString().ToLower() == "/allow")
                        {
                            CommandsPatch.Permission = player;
                        }
                    }
                }
                break;
        }
    }
}

