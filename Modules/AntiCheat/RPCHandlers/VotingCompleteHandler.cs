using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class VotingCompleteHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.VotingComplete;
}
