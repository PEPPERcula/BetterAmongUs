using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Items.Enums;
using BetterAmongUs.Modules;
using BetterAmongUs.Mono;
using Hazel;
using InnerNet;
using UnityEngine;

internal abstract class RPCHandler
{
    internal static readonly RPCHandler?[] allHandlers = [.. RegisterRPCHandlerAttribute.Instances];
    internal InnerNetClient innerNetClient => AmongUsClient.Instance;
    internal virtual byte CallId => byte.MaxValue;
    internal virtual byte GameDataTag => byte.MaxValue;
    internal virtual bool LocalHandling { get; set; }
    protected static bool CancelAsHost => !(GameState.IsHost);

    internal static bool CheckRange(Vector2 pos1, Vector2 pos2, float range) => Vector2.Distance(pos1, pos2) <= range;
    internal virtual void Handle(PlayerControl? sender, MessageReader reader) { }
    internal virtual void HandleGameData(MessageReader reader) { }
    internal virtual void HandleCheatRpcCheck(PlayerControl? sender, MessageReader reader) { }
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
                    if (handlerFlag == HandlerFlag.Handle) handler.Handle(sender, reader);
                    else if (handlerFlag == HandlerFlag.AntiCheatCancel) cancel = !handler.HandleAntiCheatCancel(sender, reader);
                    else if (handlerFlag == HandlerFlag.AntiCheat) handler.HandleAntiCheat(sender, reader);
                    else if (handlerFlag == HandlerFlag.CheatRpcCheck) handler.HandleCheatRpcCheck(sender, reader);
                    else if (handlerFlag == HandlerFlag.BetterHost) cancel = !handler.BetterHandle(sender, reader);
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
                    handler.HandleGameData(reader);
                }
                catch (Exception ex)
                {
                    Logger_.Error(ex);
                }
            }
        }

        return !(cancel);
    }

    internal void LogRpcInfo(string info, PlayerControl? player = null)
    {
        string Name = Enum.GetName((RpcCalls)CallId) ?? Enum.GetName((CustomRPC)CallId) ?? $"Unregistered({CallId})";
        Name = $"[{Enum.GetName(catchedHandlerFlag)}] > " + Name;
        Logger_.LogCheat($"{catchedSender?.BetterData()?.RealName ?? player.BetterData()?.RealName ?? string.Empty} {Name}: {info}");
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