using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class CloseMeetingHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.CloseMeeting;
}
