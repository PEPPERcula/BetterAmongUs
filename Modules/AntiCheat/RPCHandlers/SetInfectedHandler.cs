using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class SetInfectedHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetInfected;
}
