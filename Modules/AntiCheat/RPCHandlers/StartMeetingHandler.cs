using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class StartMeetingHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.StartMeeting;
}
