using AmongUs.GameOptions;
using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

public class EnterVentHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.EnterVent;

    public override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        if (!sender.IsImpostorTeam() && Role != RoleTypes.Engineer)
        {
            BetterNotificationManager.NotifyCheat(sender, GetFormatActionText());
            LogRpcInfo($"{sender.IsImpostorTeam()} && {Role != RoleTypes.Engineer} && {sender.IsAlive()}");
        }
    }
}
