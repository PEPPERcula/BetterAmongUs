using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

public class CheckNameHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.CheckName;

    public override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader) => GetHandlerInstance<SetNameHandler>().HandleAntiCheatCancel(sender, reader);
}
