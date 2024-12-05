namespace BetterAmongUs.Modules.AntiCheat;

public class SyncSettingsHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SyncSettings;
}
