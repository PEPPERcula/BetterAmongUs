using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Managers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class SendQuickChatHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SendQuickChat;

    public override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        if (sender.IsAlive() && GameState.IsInGamePlay && !GameState.IsMeeting && !GameState.IsExilling || reader.BytesRemaining == 0)
        {
            BetterNotificationManager.NotifyCheat(sender, GetFormatActionText(), forceBan: reader.BytesRemaining == 0);
            LogRpcInfo($"{sender.IsAlive()} && {GameState.IsInGamePlay} && {!GameState.IsMeeting} && {!GameState.IsExilling} || {reader.BytesRemaining == 0}");
        }
    }
}
