using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class ExiledHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.Exiled;
}
