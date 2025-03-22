using AmongUs.GameOptions;
using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class CheckProtectHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.CheckProtect;

    public override bool BetterHandle(PlayerControl? sender, MessageReader reader)
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

    public override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        if (!GameState.IsHost)
        {
            return false;
        }

        return true;
    }
}
