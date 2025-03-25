using AmongUs.GameOptions;
using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Managers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class EnterVentHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.EnterVent;

    internal override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        if (!sender.IsImpostorTeam() && !sender.Is(RoleTypes.Engineer))
        {
            BetterNotificationManager.NotifyCheat(sender, GetFormatActionText());
            LogRpcInfo($"{sender.IsImpostorTeam()} && {!sender.Is(RoleTypes.Engineer)}");
        }
    }
}
