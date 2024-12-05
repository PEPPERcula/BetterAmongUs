namespace BetterAmongUs.Modules.AntiCheat;

public class SetRoleHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SetRole;
}
