using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

public class ExitVentHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.ExitVent;

    public override void HandleAntiCheat(PlayerControl? sender, MessageReader reader) => GetHandlerInstance<EnterVentHandler>().HandleAntiCheat(sender, reader);
}
