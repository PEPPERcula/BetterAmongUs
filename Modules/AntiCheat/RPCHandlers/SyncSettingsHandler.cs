using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class SyncSettingsHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SyncSettings;
}
