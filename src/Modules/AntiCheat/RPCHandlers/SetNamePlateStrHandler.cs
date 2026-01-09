using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class SetNamePlateStrHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetNamePlateStr;
}
