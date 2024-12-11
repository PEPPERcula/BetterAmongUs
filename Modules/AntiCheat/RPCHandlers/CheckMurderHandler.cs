using BetterAmongUs.Helpers;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat;

public class CheckMurderHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.CheckMurder;

    public override bool BetterHandle(PlayerControl? sender, MessageReader reader)
    {
        PlayerControl target = reader.ReadNetObject<PlayerControl>();

        if (target != null)
        {
            if (sender.IsAlive()
                && sender.IsImpostorTeam()
                && !sender.inMovingPlat
                && !sender.IsInVent()
                && !sender.IsInVanish()
                && !sender.shapeshifting
                && !sender.onLadder
                && !sender.MyPhysics.Animations.IsPlayingAnyLadderAnimation()
                && CheckRange(sender.GetCustomPosition(), target.GetCustomPosition(), 3f))
            {
                if (target.IsAlive()
                    && !target.IsImpostorTeam()
                    && !target.inMovingPlat
                    && !target.IsInVent()
                    && !target.onLadder
                    && !target.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
                {
                    sender.RpcMurderPlayer(target, true);
                }
            }
        }

        return false;
    }
}
