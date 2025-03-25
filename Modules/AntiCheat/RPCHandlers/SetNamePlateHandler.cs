using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class SetNamePlateHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetNamePlate;
}
