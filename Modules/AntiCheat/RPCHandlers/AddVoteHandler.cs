namespace BetterAmongUs.Modules.AntiCheat;

public class AddVoteHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.AddVote;
}
