using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class ExiledHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.Exiled;
}
