namespace BetterAmongUs.Modules.AntiCheat;

public class CastVoteHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.CastVote;
}
