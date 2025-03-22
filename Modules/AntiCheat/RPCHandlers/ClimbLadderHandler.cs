using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class ClimbLadderHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.ClimbLadder;
}
