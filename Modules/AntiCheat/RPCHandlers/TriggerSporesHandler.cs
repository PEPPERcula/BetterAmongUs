using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class TriggerSporesHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.TriggerSpores;
}
