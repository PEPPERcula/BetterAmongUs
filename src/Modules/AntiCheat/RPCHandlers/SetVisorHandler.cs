using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class SetVisorHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetVisor_Deprecated;
}
