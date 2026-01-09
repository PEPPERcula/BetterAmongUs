using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class BootFromVentHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.BootFromVent;
}
