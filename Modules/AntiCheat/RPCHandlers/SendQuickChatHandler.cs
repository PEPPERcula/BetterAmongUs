using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

public class SendQuickChatHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SendQuickChat;

    public override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        if (sender.IsAlive() && GameState.IsInGamePlay && !GameState.IsMeeting && !GameState.IsExilling)
        {
            BetterNotificationManager.NotifyCheat(sender, GetFormatActionText());
            LogRpcInfo($"{sender.IsAlive()} && {GameState.IsInGamePlay} && {!GameState.IsMeeting} && {!GameState.IsExilling}");
        }
    }
}
