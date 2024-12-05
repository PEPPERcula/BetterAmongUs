namespace BetterAmongUs.Modules.AntiCheat;

public class CheckMurderHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.CheckMurder;
}
