using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class ClearVoteHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.ClearVote;
}
