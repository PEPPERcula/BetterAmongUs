using AmongUs.GameOptions;
using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat;

public class ShapeshiftHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.Shapeshift;

    public override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        var target = reader.ReadNetObject<PlayerControl>();
        var flag = reader.ReadBoolean();

        if (Role is not RoleTypes.Shapeshifter || !sender.IsAlive())
        {
            BetterNotificationManager.NotifyCheat(sender, GetFormatActionText());
            LogRpcInfo($"{Role is not RoleTypes.Shapeshifter} || {!sender.IsAlive()}");
            return false;
        }
        else if (!flag && !GameState.IsMeeting && !GameState.IsExilling && !sender.IsInVent())
        {
            BetterNotificationManager.NotifyCheat(sender, GetFormatActionText());
            LogRpcInfo($"{!flag} && {!GameState.IsMeeting} && {!GameState.IsExilling} && {!sender.IsInVent()}");
            return false;
        }

        return true;
    }
}
