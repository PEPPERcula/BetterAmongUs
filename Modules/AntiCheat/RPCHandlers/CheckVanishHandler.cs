namespace BetterAmongUs.Modules.AntiCheat;

public class CheckVanishHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.CheckVanish;
}
