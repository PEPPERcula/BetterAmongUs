using BetterAmongUs.Data;
using BetterAmongUs.Helpers;
using Hazel;
using System.Collections;
using UnityEngine;

namespace BetterAmongUs.Modules;

internal class HandshakeHandler(ExtendedPlayerInfo extendedPlayerInfo)
{
    private readonly ExtendedPlayerInfo extendedData = extendedPlayerInfo;

    internal IEnumerator CoSendSecretToPlayer()
    {
        if (!Main.SendBetterRpc.Value) yield break;

        while (extendedData._Data?.Object == null || PlayerControl.LocalPlayer == null)
        {
            if (GameState.IsFreePlay) yield break;
            yield return null;
        }
        yield return new WaitForSeconds(1f);

        SendSecretToPlayer();
    }

    internal void ResendSecretToPlayer()
    {
        if (!Main.SendBetterRpc.Value) return;
        if (HasSendSharedSecret && extendedData.IsVerifiedBetterUser) return;

        HasSendSharedSecret = false;
        SendSecretToPlayer();
    }

    // Local client sends to client
    private void SendSecretToPlayer()
    {
        if (extendedData._Data.Object.IsLocalPlayer()) return;
        if (HasSendSharedSecret) return;

        HasSendSharedSecret = true;
        var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SendSecretToPlayer, SendOption.Reliable, extendedData._Data.ClientId);
        writer.WriteBytes(SharedSecret.GetPublicKey());
        writer.Write(SharedSecret.GetTempKey());
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    // Client receives from local client
    internal void HandleSecretFromSender(MessageReader reader)
    {
        if (extendedData._Data?.Object?.IsLocalPlayer() == true) return;

        byte[] sendersPublicKey = reader.ReadBytes();
        int tempKey = reader.ReadInt32();

        // Logger.Log($"Received public key ({sendersPublicKey.Length} bytes) from {_Data.PlayerName}");

        byte[] secret = SharedSecret.GenerateSharedSecret(sendersPublicKey);
        if (secret.Length == 0)
        {
            // Logger.Error("Failed to generate shared secret!");
            return;
        }
        extendedData.IsBetterUser = true;
        TryHandlePendingVerificationData();
        extendedData._Data?.Object?.DirtyName();
        SendSecretHashToSender(tempKey, extendedData._Data.ClientId);
        ResendSecretToPlayer();
    }

    // Client sends back to local client
    private void SendSecretHashToSender(int tempKey, int senderClientId)
    {
        if (!Main.SendBetterRpc.Value) return;

        int hash = SharedSecret.GetSharedSecretHash();
        // Logger.Log($"Sending secret hash: {hash} (tempKey: {tempKey})");

        var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CheckSecretHashFromPlayer, SendOption.Reliable, senderClientId);
        writer.Write(tempKey);
        writer.Write(hash);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    internal void HandleSecretHashFromPlayer(MessageReader reader)
    {
        int tempKey = reader.ReadInt32();
        int receivedHash = reader.ReadInt32();
        _pendingVerificationData = (tempKey, receivedHash);
        TryHandlePendingVerificationData();
    }

    internal void TryHandlePendingVerificationData()
    {
        if (_pendingVerificationData?.tempKey == null || _pendingVerificationData?.receivedHash == null) return;
        if (SharedSecret.GetSharedSecret().Length == 0) return;

        var tempKey = _pendingVerificationData?.tempKey;
        var receivedHash = _pendingVerificationData?.receivedHash;

        // Logger.Log($"Received hash check: TempKey={tempKey} (ours={SharedSecret.GetTempKey()}), Hash={receivedHash} (ours={SharedSecret.GetSharedSecretHash()})");

        if (tempKey != SharedSecret.GetTempKey())
        {
            // Logger.Warning($"Invalid tempKey from {_Data.PlayerName}");
            return;
        }

        extendedData.IsBetterUser = true;

        if (receivedHash == SharedSecret.GetSharedSecretHash())
        {
            extendedData.IsVerifiedBetterUser = true;
            extendedData._Data.Object?.DirtyName();
            // Logger.Log($"Verified player: {_Data.PlayerName}");
        }
        else
        {
            // Logger.Warning($"Hash mismatch from {_Data.PlayerName}");
        }

        _pendingVerificationData = null;
    }

    private (int tempKey, int receivedHash)? _pendingVerificationData = null;
    private bool HasSendSharedSecret { get; set; }
    internal SharedSecretExchange SharedSecret { get; set; } = new();
}
