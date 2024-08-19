using BetterAmongUs.Patches;
using HarmonyLib;
using Hazel;
using InnerNet;

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

        var canceled = false;
        if (GameStates.IsHost)
        {
            if (BetterHostManager.CheckRPCAsHost(__instance, callId, reader, ref canceled) != true)
            {
                if (canceled)
                {
                    Logger.LogCheat($"RPC canceled by Host: {Enum.GetName((RpcCalls)callId)}{Enum.GetName((CustomRPC)callId)} - {callId}");
                }
                return false;
            }
        }

        AntiCheat.CheckRPC(__instance, callId, reader);
        RPC.HandleRPC(__instance, callId, reader);

        return true;
    }
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        RPC.HandleCustomRPC(__instance, callId, reader);
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.UpdateSystem), typeof(SystemTypes), typeof(PlayerControl), typeof(MessageReader))]
public static class MessageReaderUpdateSystemPatch
{
    public static bool Prefix(/*ShipStatus __instance,*/ [HarmonyArgument(0)] SystemTypes systemType, [HarmonyArgument(1)] PlayerControl player, [HarmonyArgument(2)] MessageReader reader)
    {
        if (GameStates.IsHideNSeek) return false;
        
        var amount = MessageReader.Get(reader).ReadByte();
        if (AntiCheat.RpcUpdateSystemCheck(player, systemType, amount) != true)
        {
            Logger.LogCheat($"RPC canceled by Anti-Cheat: {Enum.GetName(typeof(SystemTypes), (int)systemType)} - {amount}");
            return false;
        }

        return true;
    }
}

internal static class RPC
{
    public static void SendBetterCheck()
    {
        var flag = GameStates.IsHost && Main.BetterHost.Value;
        MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.BetterCheck, SendOption.None, -1);
        messageWriter.Write(true);
        messageWriter.Write(flag);
        messageWriter.Write(Main.GetVersionText().Replace(" ", ""));
        AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
    }

    public static void SetNamePrivate(PlayerControl player, string name, PlayerControl target)
    {
        if (!GameStates.IsHost || !GameStates.IsVanillaServer)
        {
            return;
        }

        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetName, SendOption.Reliable, target.GetClientId());
        writer.Write(player.Data.NetId);
        writer.Write(name);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    public static void SyncAllNames(bool isForMeeting = false, bool force = false, bool isBetterHost = true)
    {
        if (!GameStates.IsHost || !GameStates.IsVanillaServer)
        {
            return;
        }

        foreach (PlayerControl player in Main.AllPlayerControls)
            BetterHostManager.SetPlayersInfoAsHost(player, isForMeeting, force, isBetterHost);
    }

    public static void ExileAsync(PlayerControl player)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.Exiled, SendOption.Reliable, -1);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        player.Exiled();
    }

    public static void SendHostChatToPlayer(PlayerControl player, string text, string? title = "", bool sendToBetterUser = true)
    {
        if (!GameStates.IsHost) return;

        if (player.BetterData().IsBetterUser)
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

        /*
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

        _ = new LateTask(() =>
        {
            MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(asPlayer.NetId, (byte)RpcCalls.SetName, SendOption.None, player.GetClientId());
            writer2.Write(asPlayer.Data.NetId);
            writer2.Write(oldName);
            AmongUsClient.Instance.FinishRpcImmediately(writer2);

            SyncAllNames(force: true);
        }, 0.08f, shoudLog: false); 
        */
    }

    public static void HandleCustomRPC(PlayerControl player, byte callId, MessageReader oldReader)
    {
        if (PlayerControl.LocalPlayer == null || player == null || player == PlayerControl.LocalPlayer || player.Data == null) return;

        if (!Enum.IsDefined(typeof(CustomRPC), (int)unchecked(callId))) return;

        MessageReader reader = MessageReader.Get(oldReader);

        switch (callId)
        {
            case (byte)CustomRPC.BetterCheck:
                {
                    var SetBetterUser = reader.ReadBoolean();
                    var IsBetterHost = reader.ReadBoolean();

                    if (!player.IsHost() && IsBetterHost)
                    {
                        BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((CustomRPC)callId)} called as BetterHost");
                        break;
                    }

                    player.BetterData().IsBetterUser = SetBetterUser;
                    player.BetterData().IsBetterHost = IsBetterHost;
                }
                break;
            case (byte)CustomRPC.AddChat:
                Utils.AddChatPrivate(reader.ReadString(), reader.ReadString());
                break;
            case (byte)CustomRPC.VersionCheck or (byte)CustomRPC.RequestRetryVersionCheck:
                if (player.IsHost())
                {
                    player.BetterData().IsTOHEHost = true;
                    var BAU = "<color=#278720>♻</color><color=#0ed400><b>BetterAmongUs</b></color><color=#278720>♻</color>";
                    Utils.DisconnectSelf($"{BAU} does not support <color=#ff9cdc><b>TOHE</b></color>");
                }
                break;
        }
    }

    public static void HandleRPC(PlayerControl player, byte callId, MessageReader oldReader)
    {
        if (PlayerControl.LocalPlayer == null || player == null || player == PlayerControl.LocalPlayer || player.Data == null) return;

        MessageReader reader = MessageReader.Get(oldReader);

        switch (callId)
        {
            case (byte)RpcCalls.SendChat:
                var text = reader.ReadString();

                if (player.IsHost() && player != PlayerControl.LocalPlayer)
                {
                    if (text.ToLower() == "/allow")
                    {
                        CommandsPatch.Permission = player;
                        BetterNotificationManager.Notify($"{player.GetPlayerNameAndColor()} has granted permission!");
                    }
                }
                break;
        }
    }
}

