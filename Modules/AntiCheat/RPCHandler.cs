using BetterAmongUs.Helpers;
using Hazel;
using System.Reflection;

namespace BetterAmongUs.Modules.AntiCheat;

public enum HandlerFlag
{
    Handle,
    AntiCheatCheck,
    AntiCheatCancel,
    AntiCheat,
    BetterHost,
}

public abstract class RPCHandler
{
    public static T? GetHandlerInstance<T>() where T : RPCHandler => allHandlers.FirstOrDefault(handler => handler.GetType() == typeof(T)) as T;
    public static readonly RPCHandler?[] allHandlers = GetAllHandlerInstances();

    public static RPCHandler?[] GetAllHandlerInstances() => Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t => t.IsSubclassOf(typeof(RPCHandler)) && !t.IsAbstract)
        .Select(t => (RPCHandler?)Activator.CreateInstance(t))
        .ToArray();

    public abstract byte CallId { get; }
    public virtual bool LocalHandling { get; set; }
    protected static bool CancelAsHost => !(GameState.IsHost);

    public virtual void Handle(PlayerControl? sender, MessageReader reader) { }
    public virtual void HandleAntiCheatCheck(PlayerControl? sender, MessageReader reader) { }
    public virtual bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader) => true;
    public virtual void HandleAntiCheat(PlayerControl? sender, MessageReader reader) { }
    public virtual bool BetterHandle(PlayerControl? sender, MessageReader reader) => true;

    protected static PlayerControl? catchedSender;
    protected static HandlerFlag catchedHandlerFlag = HandlerFlag.Handle;
    public static bool HandleRPC(byte calledId, PlayerControl? sender, MessageReader reader, HandlerFlag handlerFlag)
    {
        catchedSender = sender;
        catchedHandlerFlag = handlerFlag;
        bool cancel = false;

        foreach (var handler in allHandlers)
        {
            if (calledId == handler.CallId)
            {
                try
                {
                    if (handlerFlag == HandlerFlag.Handle) handler.Handle(sender, MessageReader.Get(reader));
                    if (handlerFlag == HandlerFlag.AntiCheatCheck) handler.HandleAntiCheatCheck(sender, MessageReader.Get(reader));
                    if (handlerFlag == HandlerFlag.AntiCheatCancel) cancel = !handler.HandleAntiCheatCancel(sender, MessageReader.Get(reader));
                    if (handlerFlag == HandlerFlag.AntiCheat) handler.HandleAntiCheat(sender, MessageReader.Get(reader));
                    if (handlerFlag == HandlerFlag.BetterHost) cancel = !handler.BetterHandle(sender, MessageReader.Get(reader));
                    if (!(cancel)) break;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        return !(cancel);
    }

    public void LogRpcInfo(string info)
    {
        string Name = Enum.GetName((RpcCalls)CallId) ?? Enum.GetName((CustomRPC)CallId) ?? $"Unregistered({CallId})";
        Name = $"[{Enum.GetName(catchedHandlerFlag)}] > " + Name;
        Logger.LogCheat($"{catchedSender.BetterData().RealName} {Name}: {info}");
    }

    public string GetFormatActionText()
    {
        string Name = Enum.GetName((RpcCalls)CallId) ?? Enum.GetName((CustomRPC)CallId) ?? $"Unregistered({CallId})";
        return string.Format(Translator.GetString("AntiCheat.InvalidActionRPC"), Name);
    }

    public string GetFormatSetText()
    {
        string Name = Enum.GetName((RpcCalls)CallId) ?? Enum.GetName((CustomRPC)CallId) ?? $"Unregistered({CallId})";
        return string.Format(Translator.GetString("AntiCheat.InvalidSetRPC"), Name);
    }
}