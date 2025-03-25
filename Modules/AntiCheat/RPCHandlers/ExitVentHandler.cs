using BetterAmongUs.Items.Attributes;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class ExitVentHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.ExitVent;

    internal override void HandleAntiCheat(PlayerControl? sender, MessageReader reader) => RegisterRPCHandlerAttribute.GetClassInstance<EnterVentHandler>().HandleAntiCheat(sender, reader);
}
