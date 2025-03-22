using BetterAmongUs.Items.Attributes;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class ExitVentHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.ExitVent;

    public override void HandleAntiCheat(PlayerControl? sender, MessageReader reader) => RegisterRPCHandlerAttribute.GetClassInstance<EnterVentHandler>().HandleAntiCheat(sender, reader);
}
