using AmongUs.GameOptions;
using BetterAmongUs.Helpers;
using BetterAmongUs.Attributes;
using BetterAmongUs.Managers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class StartVanishHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.StartVanish;

    internal override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        if (RoleCheck(sender) == false)
        {
            return false;
        }

        if (sender.IsInVent())
        {
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatActionText()))
            {
                LogRpcInfo($"{sender.IsInVent()}");
            }
        }

        return true;
    }

    internal bool RoleCheck(PlayerControl? sender)
    {
        if (!sender.Is(RoleTypes.Phantom) || !sender.IsAlive())
        {
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatActionText()))
            {
                LogRpcInfo($"{!sender.Is(RoleTypes.Phantom)} || {!sender.IsAlive()}");
            }
            return false;
        }

        return true;
    }
}
