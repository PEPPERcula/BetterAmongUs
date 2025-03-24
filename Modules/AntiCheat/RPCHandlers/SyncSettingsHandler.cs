using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class SyncSettingsHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SyncSettings;
}
