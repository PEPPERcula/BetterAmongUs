using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class SetStartCounterHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetStartCounter;
}
