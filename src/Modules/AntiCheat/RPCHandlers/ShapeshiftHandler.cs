using AmongUs.GameOptions;
using BetterAmongUs.Helpers;
using BetterAmongUs.Attributes;
using BetterAmongUs.Managers;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class ShapeshiftHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.Shapeshift;

    internal override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        var target = reader.ReadNetObject<PlayerControl>();
        var flag = reader.ReadBoolean();

        if (!sender.Is(RoleTypes.Shapeshifter) || !sender.IsAlive())
        {
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatActionText()))
            {
                LogRpcInfo($"{!sender.Is(RoleTypes.Shapeshifter)} || {!sender.IsAlive()}");
            }
            return false;
        }
        else if (!flag && !GameState.IsMeeting && !GameState.IsExilling && !sender.IsInVent())
        {
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatActionText()))
            {
                LogRpcInfo($"{!flag} && {!GameState.IsMeeting} && {!GameState.IsExilling} && {!sender.IsInVent()}");
            }
            return false;
        }

        return true;
    }
}
