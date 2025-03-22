using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class VotingCompleteHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.VotingComplete;
}
