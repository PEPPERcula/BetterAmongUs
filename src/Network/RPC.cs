using BetterAmongUs.Enums;
using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using BetterAmongUs.Mono;
using Hazel;

namespace BetterAmongUs.Network;

internal static class RPC
{
    internal static void RpcSetNameForTarget(PlayerControl player, string name, PlayerControl target)
    {
        if (!GameState.IsHost)
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
                /*
                case (byte)CustomRPC.LegacyBetterCheck:
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
                */
                case (byte)CustomRPC.SendSecretToPlayer:
                    {
                        player.BetterData().HandshakeHandler.HandleSecretFromSender(reader);
                    }
                    break;
                case (byte)CustomRPC.CheckSecretHashFromPlayer:
                    {
                        player.BetterData().HandshakeHandler.HandleSecretHashFromPlayer(reader);
                    }
                    break;
            }
        }
        else if (!Enum.IsDefined(typeof(CustomRPC), (int)unchecked(callId)))
        {
            try
            {
                if (!GameState.IsHost)
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

