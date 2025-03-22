using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class SetVisorStrHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SetVisorStr;
}
