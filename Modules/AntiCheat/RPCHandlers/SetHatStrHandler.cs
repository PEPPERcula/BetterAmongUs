using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class SetHatStrHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetHatStr;
}
