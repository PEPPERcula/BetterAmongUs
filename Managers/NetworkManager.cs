using BepInEx.Unity.IL2CPP.Utils;
using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using BetterAmongUs.Modules.AntiCheat;
using HarmonyLib;
using Hazel;
using InnerNet;
using System.Collections;
using UnityEngine;

namespace BetterAmongUs.Managers;

internal class NetworkManager
{
    internal static InnerNetClient? InnerNetClient => AmongUsClient.Instance;

    public static void HandleGameData(MessageReader parentReader)
    {
        try
        {
            while (parentReader.Position < parentReader.Length)
            {
                MessageReader messageReader = parentReader.ReadMessageAsNewBuffer();
                int currentMessageNumber = InnerNetClient.msgNum++;
                InnerNetClient.StartCoroutine(HandleGameDataInner(messageReader, currentMessageNumber));
                RPCHandler.HandleRPC(parentReader.Tag, null, MessageReader.Get(parentReader), HandlerFlag.HandleGameDataTag);
            }
        }
        finally
        {
            parentReader.Recycle();
        }
    }

    private static IEnumerator HandleGameDataInner(MessageReader reader, int msgNum)
    {
        int attemptCount = 0;
        reader.Position = 0;
        byte tag = reader.Tag;

        switch (tag)
        {
            case 1: // Object deserialization
                yield return HandleObjectDeserialization(reader, msgNum, attemptCount);
                break;

            case 2: // RPC handling
                yield return HandleRpcCall(reader, msgNum, attemptCount);
                break;

            case 4: // Spawn handling
                InnerNetClient.StartCoroutine(InnerNetClient.CoHandleSpawn(reader));
                break;

            case 5: // Object destruction
                yield return HandleObjectDestruction(reader);
                break;

            case 7: // Client ready status
                yield return HandleClientReady(reader);
                break;

            case 6: // Scene change
                HandleSceneChange(reader);
                break;

            case 207: // Special case (ulong parsing)
                yield return HandleSpecialCase(reader);
                break;

            default: // Invalid tags
                HandleInvalidTag(reader);
                break;
        }

        yield break;
    }

    private static IEnumerator HandleObjectDeserialization(MessageReader reader, int msgNum, int initialAttemptCount)
    {
        int attemptCount = initialAttemptCount;
        try
        {
            InnerNetObject innerNetObject;
            while (true)
            {
                uint netId = reader.ReadPackedUInt32();

                if (InnerNetClient.allObjectsFast.TryGetValue(netId, out innerNetObject))
                {
                    innerNetObject.Deserialize(reader, false);
                    break;
                }

                if (InnerNetClient.DestroyedObjects.Contains(netId))
                {
                    break;
                }

                Debug.LogWarning("Stored data for " + netId.ToString());
                attemptCount++;

                if (attemptCount > 10)
                {
                    yield break;
                }

                reader.Position = 0;
                yield return Effects.Wait(0.1f);
            }
        }
        finally
        {
            reader.Recycle();
        }
    }

    private static IEnumerator HandleRpcCall(MessageReader reader, int msgNum, int initialAttemptCount)
    {
        int attemptCount = initialAttemptCount;
        try
        {
            while (true)
            {
                uint netId;
                byte rpcCall;

                try
                {
                    netId = reader.ReadPackedUInt32();
                    rpcCall = reader.ReadByte();
                }
                catch
                {
                    throw;
                }

                if (InnerNetClient.allObjectsFast.TryGetValue(netId, out InnerNetObject innerNetObject))
                {
                    if (!HandleInnerNetObject(innerNetObject, rpcCall, reader))
                    {
                        break;
                    }

                    if (Enum.IsDefined(typeof(RpcCalls), rpcCall))
                    {
                        innerNetObject.HandleRpc(rpcCall, reader);
                    }
                    else
                    {
                        if (innerNetObject is PlayerControl player)
                        {
                            RPC.HandleCustomRPC(player, rpcCall, reader);
                        }
                    }

                    break;
                }

                if (netId == 4294967295U || InnerNetClient.DestroyedObjects.Contains(netId))
                {
                    break;
                }

                Debug.LogWarning($"Stored Msg {msgNum} RPC {(RpcCalls)rpcCall} for {netId}");
                attemptCount++;

                if (attemptCount > 10)
                {
                    yield break;
                }

                reader.Position = 0;
                yield return Effects.Wait(0.1f);
            }
        }
        finally
        {
            reader.Recycle();
        }
    }

    private static IEnumerator HandleObjectDestruction(MessageReader reader)
    {
        try
        {
            uint netId = reader.ReadPackedUInt32();
            InnerNetClient.DestroyedObjects.Add(netId);

            InnerNetObject innerNetObject = InnerNetClient.FindObjectByNetId<InnerNetObject>(netId);
            if (innerNetObject && !innerNetObject.AmOwner)
            {
                InnerNetClient.RemoveNetObject(innerNetObject);
                innerNetObject.gameObject.DestroyObj();
            }
        }
        finally
        {
            reader.Recycle();
        }
        yield break;
    }

    private static IEnumerator HandleClientReady(MessageReader reader)
    {
        try
        {
            ClientData clientData = InnerNetClient.FindClientById(reader.ReadPackedInt32());
            if (clientData != null)
            {
                clientData.IsReady = true;
            }
        }
        finally
        {
            reader.Recycle();
        }
        yield break;
    }

    private static void HandleSceneChange(MessageReader reader)
    {
        int clientId = reader.ReadPackedInt32();
        ClientData clientData = InnerNetClient.FindClientById(clientId);
        string sceneName = reader.ReadString();

        if (clientData != null && !string.IsNullOrWhiteSpace(sceneName))
        {
            InnerNetClient.StartCoroutine(InnerNetClient.CoOnPlayerChangedScene(clientData, sceneName));
        }
        else
        {
            Debug.Log($"Couldn't find client {clientId} to change scene to {sceneName}");
            reader.Recycle();
        }
    }

    private static IEnumerator HandleSpecialCase(MessageReader reader)
    {
        try
        {
            ulong.Parse(reader.ReadString());
        }
        finally
        {
            reader.Recycle();
        }
        yield break;
    }

    private static void HandleInvalidTag(MessageReader reader)
    {
        Debug.Log($"Bad tag {reader.Tag} at {reader.Offset}+{reader.Position}={reader.Length}: " +
                  string.Join(" ", reader.Buffer.Take(128)));
        reader.Recycle();
    }

    private static bool HandleInnerNetObject(InnerNetObject netObj, byte callId, MessageReader reader)
    {
        if (netObj is PlayerControl player)
        {
            if (!PlayerRpc(player, callId, reader))
            {
                return false;
            }
        }
        else if (netObj is PlayerPhysics physics)
        {
            if (!PlayerRpc(physics.myPlayer, callId, reader))
            {
                return false;
            }
        }

        return true;
    }

    private static bool PlayerRpc(PlayerControl player, byte callId, MessageReader reader)
    {
        player.BetterData().AntiCheatInfo.RPCSentPS++;
        if (player.BetterData().AntiCheatInfo.RPCSentPS >= ExtendedAntiCheatInfo.MaxRPCSent)
        {
            return false;
        }

        BAUAntiCheat.HandleCheatRPCBeforeCheck(player, callId, reader);

        if (BAUAntiCheat.CheckCancelRPC(player, callId, reader) != true)
        {
            Logger.LogCheat($"RPC canceled by Anti-Cheat: {Enum.GetName((RpcCalls)callId)}{Enum.GetName((CustomRPC)callId)} - {callId}");
            return false;
        }

        BAUAntiCheat.CheckRPC(player, callId, reader);
        RPC.HandleRPC(player, callId, reader);

        return true;
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.UpdateSystem), typeof(SystemTypes), typeof(PlayerControl), typeof(MessageReader))]
    internal static class MessageReaderUpdateSystemPatch
    {
        internal static bool Prefix(/*ShipStatus __instance,*/ [HarmonyArgument(0)] SystemTypes systemType, [HarmonyArgument(1)] PlayerControl player, [HarmonyArgument(2)] MessageReader reader)
        {
            player.BetterData().AntiCheatInfo.RPCSentPS++;
            if (player.BetterData().AntiCheatInfo.RPCSentPS >= ExtendedAntiCheatInfo.MaxRPCSent)
            {
                return false;
            }

            if (BAUAntiCheat.RpcUpdateSystemCheck(player, systemType, reader) != true)
            {
                Logger.LogCheat($"RPC canceled by Anti-Cheat: {Enum.GetName(typeof(SystemTypes), (int)systemType)} - {MessageReader.Get(reader).ReadByte()}");
                return false;
            }

            return true;
        }
    }
}