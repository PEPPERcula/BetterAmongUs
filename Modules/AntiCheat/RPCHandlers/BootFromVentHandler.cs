using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class BootFromVentHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.BootFromVent;
}
