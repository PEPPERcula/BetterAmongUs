using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class SnapToHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SnapTo;
}
