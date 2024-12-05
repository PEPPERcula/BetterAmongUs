namespace BetterAmongUs.Modules.AntiCheat;

public class SetInfectedHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SetInfected;
}
