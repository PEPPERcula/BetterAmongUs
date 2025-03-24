using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class SetScannerHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetScanner;
}
