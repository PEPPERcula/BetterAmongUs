using AmongUs.GameOptions;
using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class CheckVanishHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.CheckVanish;

    internal override bool BetterHandle(PlayerControl? player, MessageReader reader)
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

    internal override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        if (!GameState.IsHost)
        {
            return false;
        }

        return true;
    }
}
