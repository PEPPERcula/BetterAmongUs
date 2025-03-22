using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class LobbyTimeExpiringHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.LobbyTimeExpiring;
}
