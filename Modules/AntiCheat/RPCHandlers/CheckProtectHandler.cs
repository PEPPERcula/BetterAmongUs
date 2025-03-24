using AmongUs.GameOptions;
using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class CheckProtectHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.CheckProtect;

    internal override bool BetterHandle(PlayerControl? sender, MessageReader reader)
    {
        PlayerControl target = reader.ReadNetObject<PlayerControl>();
        if (target != null)
        {
            if (sender.Is(RoleTypes.GuardianAngel)
                && !sender.IsAlive()
                && !sender.IsImpostorTeam()
                && CheckRange(sender.GetCustomPosition(), target.GetCustomPosition(), 3f))
            {
                if (target.IsAlive())
                {
                    sender.RpcProtectPlayer(target, sender.Data.DefaultOutfit.ColorId); ;
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
