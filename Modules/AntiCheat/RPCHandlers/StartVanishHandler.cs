using AmongUs.GameOptions;
using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Managers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class StartVanishHandler : RPCHandler
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
        if (!sender.Is(RoleTypes.Phantom) || !sender.IsAlive())
        {
            BetterNotificationManager.NotifyCheat(sender, GetFormatActionText());
            LogRpcInfo($"{!sender.Is(RoleTypes.Phantom)} || {!sender.IsAlive()}");
            return false;
        }

        return true;
    }
}
