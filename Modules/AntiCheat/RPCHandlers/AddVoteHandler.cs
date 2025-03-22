using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class AddVoteHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.AddVote;
}
