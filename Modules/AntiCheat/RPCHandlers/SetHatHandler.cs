using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class SetHatHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SetHat;
}
