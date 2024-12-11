using AmongUs.GameOptions;
using BetterAmongUs.Helpers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

public class CheckVanishHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.CheckVanish;

    public override bool BetterHandle(PlayerControl? player, MessageReader reader)
    {
        if (player.Is(RoleTypes.Phantom)
            && player.IsAlive()
            && player.IsImpostorTeam()
            && !player.IsInVent()
            && !player.inMovingPlat
            && !player.onLadder
            && !player.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
        {

            if (AmongUsClient.Instance.AmClient)
            {
                player.SetRoleInvisibility(true, true, true);
            }
            player.RpcVanish();
        }

        return false;
    }
}
