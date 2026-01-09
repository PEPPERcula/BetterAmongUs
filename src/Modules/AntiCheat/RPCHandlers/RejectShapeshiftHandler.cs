using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class RejectShapeshiftHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.RejectShapeshift;
}
