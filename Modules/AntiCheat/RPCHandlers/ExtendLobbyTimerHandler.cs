using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class ExtendLobbyTimerHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.ExtendLobbyTimer;
}
