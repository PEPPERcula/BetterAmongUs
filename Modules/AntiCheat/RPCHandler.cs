using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using Hazel;
using InnerNet;
using System.Reflection;
using UnityEngine;

namespace BetterAmongUs.Modules.AntiCheat;

internal enum HandlerFlag
{
    Handle,
    AntiCheatCheck,
    AntiCheatCancel,
    AntiCheat,
    BetterHost,
    HandleGameDataTag
}

internal abstract class RPCHandler
{
    internal static readonly RPCHandler?[] allHandlers = [.. RegisterRPCHandlerAttribute.Instances];

    internal static RPCHandler?[] GetAllHandlerInstances() => Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t => t.IsSubclassOf(typeof(RPCHandler)) && !t.IsAbstract)
        .Select(t => (RPCHandler?)Activator.CreateInstance(t))
        .ToArray();

    internal InnerNetClient innerNetClient => DestroyableSingleton<InnerNetClient>.Instance;
    internal virtual byte CallId => byte.MaxValue;
    internal virtual byte GameDataTag => byte.MaxValue;
    internal virtual bool LocalHandling { get; set; }
    protected static bool CancelAsHost => !(GameState.IsHost);

    internal static bool CheckRange(Vector2 pos1, Vector2 pos2, float range) => Vector2.Distance(pos1, pos2) <= range;
    internal virtual void Handle(PlayerControl? sender, MessageReader reader) { }
    internal virtual void HandleGameData(MessageReader reader) { }
    internal virtual void HandleAntiCheatCheck(PlayerControl? sender, MessageReader reader) { }
    internal virtual bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader) => true;
    internal virtual void HandleAntiCheat(PlayerControl? sender, MessageReader reader) { }
    internal virtual bool BetterHandle(PlayerControl? sender, MessageReader reader) => true;

    protected static PlayerControl? catchedSender;
    protected static HandlerFlag catchedHandlerFlag = HandlerFlag.Handle;
    internal static bool HandleRPC(byte calledId, PlayerControl? sender, MessageReader reader, HandlerFlag handlerFlag)
    {
        catchedSender = sender;
        catchedHandlerFlag = handlerFlag;
        bool cancel = false;

        foreach (var handler in allHandlers)
        {
            if (calledId == handler.CallId && handlerFlag != HandlerFlag.HandleGameDataTag)
            {
                try
                {
                    if (handlerFlag == HandlerFlag.Handle) handler.Handle(sender, MessageReader.Get(reader));
                    else if (handlerFlag == HandlerFlag.AntiCheatCheck) handler.HandleAntiCheatCheck(sender, MessageReader.Get(reader));
                    else if (handlerFlag == HandlerFlag.AntiCheatCancel) cancel = !handler.HandleAntiCheatCancel(sender, MessageReader.Get(reader));
                    else if (handlerFlag == HandlerFlag.AntiCheat) handler.HandleAntiCheat(sender, MessageReader.Get(reader));
                    else if (handlerFlag == HandlerFlag.BetterHost) cancel = !handler.BetterHandle(sender, MessageReader.Get(reader));
                    if (!(cancel)) break;
                }
                catch
                {
                }
            }
            else if (calledId == handler.GameDataTag && handlerFlag == HandlerFlag.HandleGameDataTag)
            {
                try
                {
                    handler.HandleGameData(MessageReader.Get(reader));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        return !(cancel);
    }

    internal void LogRpcInfo(string info, PlayerControl? player = null)
    {
        string Name = Enum.GetName((RpcCalls)CallId) ?? Enum.GetName((CustomRPC)CallId) ?? $"Unregistered({CallId})";
        Name = $"[{Enum.GetName(catchedHandlerFlag)}] > " + Name;
        Logger.LogCheat($"{catchedSender?.BetterData()?.RealName ?? player.BetterData()?.RealName ?? string.Empty} {Name}: {info}");
    }

    internal string GetFormatActionText()
    {
        string Name = Enum.GetName((RpcCalls)CallId) ?? Enum.GetName((CustomRPC)CallId) ?? $"Unregistered({CallId})";
        return string.Format(Translator.GetString("AntiCheat.InvalidActionRPC"), Name);
    }

    internal string GetFormatSetText()
    {
        string Name = Enum.GetName((RpcCalls)CallId) ?? Enum.GetName((CustomRPC)CallId) ?? $"Unregistered({CallId})";
        return string.Format(Translator.GetString("AntiCheat.InvalidSetRPC"), Name);
    }
}