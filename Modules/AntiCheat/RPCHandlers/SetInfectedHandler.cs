using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class SetInfectedHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SetInfected;
}
