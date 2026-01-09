using AmongUs.GameOptions;
using BetterAmongUs.Helpers;
using BetterAmongUs.Attributes;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class CheckAppearHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.CheckAppear;

    internal override bool BetterHandle(PlayerControl? sender, MessageReader reader)
    {
        bool flag = reader.ReadBoolean();

        if (sender.Is(RoleTypes.Phantom)
            && sender.IsAlive() && sender.IsImpostorTeam()
            && !sender.inMovingPlat
            && !sender.onLadder
            && !sender.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
        {
            if (!sender.IsInVent() && flag == false)
            {
                return false;
            }

            if (AmongUsClient.Instance.AmClient)
            {
                sender.SetRoleInvisibility(false, !sender.IsInVent(), true);
            }
            sender.RpcAppear(!sender.IsInVent());
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
