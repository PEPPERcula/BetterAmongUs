namespace BetterAmongUs.Modules.AntiCheat;

public class SetStartCounterHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SetStartCounter;
}
