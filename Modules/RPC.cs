using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
using BetterAmongUs.Modules.AntiCheat;
using HarmonyLib;
using Hazel;

namespace BetterAmongUs.Modules;

enum CustomRPC : int
{
    // Cheat RPC's
    Sicko = 420, // Results in 164
    AUM = 42069, // Results in 85
    AUMChat = 101,
    KillNetwork = 250,
    KillNetworkChat = 119,

    // TOHE
    VersionCheck = 80,
    RequestRetryVersionCheck = 81,

    //Better Among Us
    BetterCheck = 150,
}

enum HandleGameDataTags : byte
{
    NetObjectDeserialize = 1,
    NetObjectHandleRPC = 2,
    NetObjectSpawn = 4,
    NetObjectDespawn = 5,
    ClientDataReady = 7,
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
internal class PlayerControlRPCHandlerPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        BAUAntiCheat.HandleCheatRPCBeforeCheck(__instance, callId, reader);

        if (BAUAntiCheat.CheckCancelRPC(__instance, callId, reader) != true)
        {
            Logger.LogCheat($"RPC canceled by Anti-Cheat: {Enum.GetName((RpcCalls)callId)}{Enum.GetName((CustomRPC)callId)} - {callId}");
            return false;
        }

        BAUAntiCheat.CheckRPC(__instance, callId, reader);
        RPC.HandleRPC(__instance, callId, reader);

        return true;
    }
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        RPC.HandleCustomRPC(__instance, callId, reader);
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
internal class PlayerPhysicsRPCHandlerPatch
{
    public static void Prefix(PlayerPhysics __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        if (BAUAntiCheat.CheckCancelRPC(__instance.myPlayer, callId, reader) != true)
        {
            Logger.LogCheat($"RPC canceled by Anti-Cheat: {Enum.GetName((RpcCalls)callId)}{Enum.GetName((CustomRPC)callId)} - {callId}");
        }

        BAUAntiCheat.CheckRPC(__instance.myPlayer, callId, reader);
        RPC.HandleRPC(__instance.myPlayer, callId, reader);
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.UpdateSystem), typeof(SystemTypes), typeof(PlayerControl), typeof(MessageReader))]
public static class MessageReaderUpdateSystemPatch
{
    public static bool Prefix(/*ShipStatus __instance,*/ [HarmonyArgument(0)] SystemTypes systemType, [HarmonyArgument(1)] PlayerControl player, [HarmonyArgument(2)] MessageReader reader)
    {
        if (BAUAntiCheat.RpcUpdateSystemCheck(player, systemType, reader) != true)
        {
            Logger.LogCheat($"RPC canceled by Anti-Cheat: {Enum.GetName(typeof(SystemTypes), (int)systemType)} - {MessageReader.Get(reader).ReadByte()}");
            return false;
        }

        return true;
    }
}

internal static class RPC
{
    public static void RpcSyncSettings()
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }

        foreach (var player in Main.AllPlayerControls.Where(pc => !pc.IsLocalPlayer()))
        {
            var optionsByteArray = GameOptionsManager.Instance.gameOptionsFactory.ToBytes(GameOptionsManager.Instance.CurrentGameOptions, false);
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SyncSettings, SendOption.None, player.GetClientId());
            messageWriter.WriteBytesAndSize(optionsByteArray);
            AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
        }
    }

    public static void SendBetterCheck()
    {
        MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.BetterCheck, SendOption.None, -1);
        messageWriter.Write(true);
        messageWriter.Write(Main.modSignature);
        messageWriter.Write(Main.GetVersionText().Replace(" ", ""));
        AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
    }

    public static void SetNamePrivate(PlayerControl player, string name, PlayerControl target)
    {
        if (!GameState.IsHost || !GameState.IsVanillaServer)
        {
            return;
        }

        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetName, SendOption.Reliable, target.GetClientId());
        writer.Write(player.Data.NetId);
        writer.Write(name);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    public static void ExileAsync(PlayerControl player)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.Exiled, SendOption.Reliable, -1);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        player.Exiled();
    }

    public static void HandleCustomRPC(PlayerControl player, byte callId, MessageReader oldReader)
    {
        if (player == null || player.IsLocalPlayer() || player.Data == null || Enum.IsDefined(typeof(RpcCalls), callId)) return;

        if (Enum.IsDefined(typeof(CustomRPC), (int)unchecked(callId)))
        {
            MessageReader reader = MessageReader.Get(oldReader);

            switch (callId)
            {
                case (byte)CustomRPC.BetterCheck:
                    {
                        var SetBetterUser = reader.ReadBoolean();
                        var Signature = reader.ReadString();
                        var Version = reader.ReadString();
                        var IsVerified = Signature == Main.modSignature;

                        if (string.IsNullOrEmpty(Signature) || string.IsNullOrEmpty(Version))
                        {
                            BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((CustomRPC)callId)} called with invalid info");
                            break;
                        }

                        player.BetterData().IsBetterUser = SetBetterUser;

                        if (IsVerified)
                        {
                            player.BetterData().IsVerifiedBetterUser = true;
                        }

                        Logger.Log($"Received better user RPC from: {player.Data.PlayerName}:{player.Data.FriendCode}:{Utils.GetHashPuid(player)} - " +
                            $"BetterUser: {SetBetterUser} - " +
                            $"Version: {Version} - " +
                            $"Verified: {IsVerified} - " +
                            $"Signature: {Signature}");

                        Utils.DirtyAllNames();
                    }
                    break;
                    /*
                case (byte)CustomRPC.VersionCheck or (byte)CustomRPC.RequestRetryVersionCheck:
                    if (player.IsHost())
                    {
                        player.BetterData().IsTOHEHost = true;
                    }
                    break;
                    */
            }
        }
        else if (!Enum.IsDefined(typeof(CustomRPC), (int)unchecked(callId)))
        {
            try
            {
                if (!GameState.IsHost && !GameState.IsTOHEHostLobby)
                {
                    if (player.IsHost())
                    {
                        var Icon = Translator.GetString("BAUMark");
                        var BAU = $"<color=#278720>{Icon}</color><color=#0ed400><b>{Translator.GetString("BAU")}</b></color><color=#278720>{Icon}</color>";
                        Utils.DisconnectSelf(string.Format(Translator.GetString("ModdedLobbyMsg"), BAU));
                    }
                }
            }
            catch { }
        }
    }

    public static void HandleRPC(PlayerControl player, byte callId, MessageReader oldReader)
    {
        if (player == null || player.IsLocalPlayer() || player.Data == null) return;

        MessageReader reader = MessageReader.Get(oldReader);

        RPCHandler.HandleRPC(callId, player, reader, HandlerFlag.Handle);
    }
}

