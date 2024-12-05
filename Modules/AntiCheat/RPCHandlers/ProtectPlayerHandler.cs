using AmongUs.GameOptions;
using BetterAmongUs.Managers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

public class ProtectPlayerHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.ProtectPlayer;

    public override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        if (Role is not RoleTypes.GuardianAngel)
        {
            BetterNotificationManager.NotifyCheat(sender, GetFormatActionText());
            LogRpcInfo($"{Role is not RoleTypes.GuardianAngel}");
        }
    }
}
