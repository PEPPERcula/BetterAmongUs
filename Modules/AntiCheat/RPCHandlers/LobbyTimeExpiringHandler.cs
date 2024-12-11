namespace BetterAmongUs.Modules.AntiCheat;

public class LobbyTimeExpiringHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.LobbyTimeExpiring;
}
