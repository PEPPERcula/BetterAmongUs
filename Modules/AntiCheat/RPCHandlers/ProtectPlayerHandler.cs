using AmongUs.GameOptions;
using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

public class ProtectPlayerHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.ProtectPlayer;

    public override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        if (!sender.Is(RoleTypes.GuardianAngel))
        {
            BetterNotificationManager.NotifyCheat(sender, GetFormatActionText());
            LogRpcInfo($"{!sender.Is(RoleTypes.GuardianAngel)}");
        }
    }
}
