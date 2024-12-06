using AmongUs.GameOptions;
using BetterAmongUs.Helpers;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat;

public class CheckShapeshiftHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.CheckShapeshift;

    public override bool BetterHandle(PlayerControl? sender, MessageReader reader)
    {
        PlayerControl target = reader.ReadNetObject<PlayerControl>();
        bool flag = reader.ReadBoolean();

        if (target != null)
        {
            if (sender.Is(RoleTypes.Shapeshifter)
                && sender.IsAlive()
                && sender.IsImpostorTeam()
                && !sender.inMovingPlat
                && !sender.shapeshifting
                && !sender.onLadder
                && !sender.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
            {
                if (!sender.IsInVent() && !GameState.IsMeeting && !GameState.IsExilling && flag == false)
                {
                    return false;
                }

                sender.RpcShapeshift(target, !sender.IsInVent() && !GameState.IsMeeting && !GameState.IsExilling);
            }
        }

        return false;
    }
}
