using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class SetSkinHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SetSkin;
}
