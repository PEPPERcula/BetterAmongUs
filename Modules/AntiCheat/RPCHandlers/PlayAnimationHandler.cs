using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class PlayAnimationHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.PlayAnimation;
}
