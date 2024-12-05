using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

public class CloseDoorsOfTypeHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.CloseDoorsOfType;
    public override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        if (!sender.IsImpostorTeam())
        {
            BetterNotificationManager.NotifyCheat(sender, GetFormatActionText());
            LogRpcInfo($"{!sender.IsImpostorTeam()}");
        }
    }
}
