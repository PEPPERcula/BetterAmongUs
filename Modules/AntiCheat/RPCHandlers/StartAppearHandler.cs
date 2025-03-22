using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Managers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class StartAppearHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.StartAppear;

    public override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        bool shouldAnimate = reader.ReadBoolean();
        if (RegisterRPCHandlerAttribute.GetClassInstance<StartVanishHandler>().RoleCheck(sender) == false)
        {
            return false;
        }

        if (!shouldAnimate && (!sender.IsInVent() && !GameState.IsMeeting && !GameState.IsExilling))
        {
            BetterNotificationManager.NotifyCheat(sender, GetFormatActionText());
            LogRpcInfo($"{!shouldAnimate} && ({!sender.IsInVent()} && {!GameState.IsMeeting} && {!GameState.IsExilling})");

            sender.HandleServerAppear(true);
            return false;
        }

        return true;
    }
}
