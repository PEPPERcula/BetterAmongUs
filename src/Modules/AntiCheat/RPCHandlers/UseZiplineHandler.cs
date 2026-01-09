using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class UseZiplineHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.UseZipline;
}
