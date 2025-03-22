using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class SetNamePlateStrHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SetNamePlateStr;
}
