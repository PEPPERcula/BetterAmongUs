namespace BetterAmongUs.Modules.AntiCheat;

public class ClearVoteHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.ClearVote;
}
