using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class SetVisorHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SetVisor;
}
