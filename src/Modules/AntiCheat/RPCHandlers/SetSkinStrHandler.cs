using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class SetSkinStrHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetSkinStr;
}
