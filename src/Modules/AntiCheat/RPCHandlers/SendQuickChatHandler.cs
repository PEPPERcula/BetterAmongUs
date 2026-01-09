using BetterAmongUs.Helpers;
using BetterAmongUs.Attributes;
using BetterAmongUs.Managers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class SendQuickChatHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SendQuickChat;

    internal override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        if (sender.IsAlive() && GameState.IsInGamePlay && !GameState.IsMeeting && !GameState.IsExilling || reader.BytesRemaining == 0)
        {
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatActionText(), forceBan: reader.BytesRemaining == 0))
            {
                LogRpcInfo($"{sender.IsAlive()} && {GameState.IsInGamePlay} && {!GameState.IsMeeting} && {!GameState.IsExilling} || {reader.BytesRemaining == 0}");
            }
        }
    }
}
