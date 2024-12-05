namespace BetterAmongUs.Modules.AntiCheat;

public class CheckProtectHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.CheckProtect;
}
