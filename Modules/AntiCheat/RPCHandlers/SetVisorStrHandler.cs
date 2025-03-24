using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class SetVisorStrHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetVisorStr;
}
