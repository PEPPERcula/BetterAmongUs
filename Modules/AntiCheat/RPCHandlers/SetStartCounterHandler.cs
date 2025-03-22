using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class SetStartCounterHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SetStartCounter;
}
