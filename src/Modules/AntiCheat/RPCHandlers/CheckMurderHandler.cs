using BetterAmongUs.Helpers;
using BetterAmongUs.Attributes;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class CheckMurderHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.CheckMurder;

    internal override bool BetterHandle(PlayerControl? sender, MessageReader reader)
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

    internal override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        if (!GameState.IsHost)
        {
            return false;
        }

        return true;
    }
}
