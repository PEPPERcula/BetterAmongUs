using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class UsePlatformHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.UsePlatform;
}
