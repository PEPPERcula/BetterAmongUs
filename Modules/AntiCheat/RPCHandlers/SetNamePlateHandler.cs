using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class SetNamePlateHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SetNamePlate;
}
