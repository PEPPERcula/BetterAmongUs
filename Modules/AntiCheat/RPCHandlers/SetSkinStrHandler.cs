using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class SetSkinStrHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SetSkinStr;
}
