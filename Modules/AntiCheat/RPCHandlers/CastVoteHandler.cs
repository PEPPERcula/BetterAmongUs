using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class CastVoteHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.CastVote;
}
