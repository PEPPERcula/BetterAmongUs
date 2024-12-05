using AmongUs.GameOptions;
using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

public class StartVanishHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.StartVanish;

    public override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        if (RoleCheck(sender) == false)
        {
            return false;
        }

        if (sender.IsInVent())
        {
            BetterNotificationManager.NotifyCheat(sender, GetFormatActionText());
            LogRpcInfo($"{sender.IsInVent()}");
        }

        return true;
    }

    public bool RoleCheck(PlayerControl? sender)
    {
        if (Role is not RoleTypes.Phantom || !sender.IsAlive())
        {
            BetterNotificationManager.NotifyCheat(sender, GetFormatActionText());
            LogRpcInfo($"{Role is not RoleTypes.Phantom} || {!sender.IsAlive()}");
            return false;
        }

        return true;
    }
}
