using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
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

internal static class RPC
{
    internal static void RpcSyncSettings()
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

    internal static void RpcBetterCheck()
    {
        if (!Main.SendBetterRpc.Value) return;

        MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.BetterCheck, SendOption.None, -1);
        messageWriter.Write(true);
        messageWriter.Write(Main.ModSignature.ToString());
        messageWriter.Write(Main.GetVersionText().Replace(" ", ""));
        AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
    }

    internal static void RpcSetNamePrivate(PlayerControl player, string name, PlayerControl target)
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

    internal static void RpcExile(PlayerControl player)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.Exiled, SendOption.Reliable, -1);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        player.Exiled();
    }

    internal static void HandleCustomRPC(PlayerControl player, byte callId, MessageReader oldReader)
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
                        var IsVerified = Signature == Main.ModSignature.ToString();

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
}

