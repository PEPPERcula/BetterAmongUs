using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

public class ReportDeadBodyHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.ReportDeadBody;

    public override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        if (!GameState.IsInGamePlay || !Main.AllPlayerControls.All(pc => pc.roleAssigned))
        {
            BetterNotificationManager.NotifyCheat(sender, string.Format(Translator.GetString("AntiCheat.InvalidActionRPC"), Enum.GetName((RpcCalls)CallId)), forceBan: true);
            LogRpcInfo($"{!GameState.IsInGamePlay} || {!Main.AllPlayerControls.All(pc => pc.roleAssigned)}");

            return CancelAsHost;
        }

        if (GameState.IsMeeting && MeetingHudPatch.timeOpen > 0.5f || GameState.IsHideNSeek || sender.IsInVent() || sender.shapeshifting
            || sender.inMovingPlat || sender.onLadder || sender.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
        {
            BetterNotificationManager.NotifyCheat(sender, string.Format(Translator.GetString("AntiCheat.InvalidActionRPC"), Enum.GetName((RpcCalls)CallId)));
            LogRpcInfo($"{GameState.IsMeeting} && {MeetingHudPatch.timeOpen > 0.5f} || {sender.IsInVent()} || {sender.shapeshifting}" +
                $" || {sender.inMovingPlat} || {sender.onLadder} || {sender.MyPhysics.Animations.IsPlayingAnyLadderAnimation()}");

            return CancelAsHost;
        }

        var deadPlayerInfo = reader.ReadPlayerDataId();
        bool isBodyReport = deadPlayerInfo != null;

        if (isBodyReport)
        {
            if (!deadPlayerInfo.IsDead || deadPlayerInfo == sender.Data)
            {
                BetterNotificationManager.NotifyCheat(sender, string.Format(Translator.GetString("AntiCheat.InvalidActionRPC"), Enum.GetName((RpcCalls)CallId)));
                LogRpcInfo($"{!deadPlayerInfo.IsDead} || {deadPlayerInfo == sender.Data}");

                return CancelAsHost;
            }
        }
        else
        {
            if (sender.RemainingEmergencies <= 0)
            {
                BetterNotificationManager.NotifyCheat(sender, string.Format(Translator.GetString("AntiCheat.InvalidActionRPC"), Enum.GetName((RpcCalls)CallId)));
                LogRpcInfo($"{sender.RemainingEmergencies} -> {GameOptionsManager.Instance.currentNormalGameOptions.NumEmergencyMeetings}" +
                    $" - {sender.RemainingEmergencies <= 0}");

                return CancelAsHost;
            }
        }

        return true;
    }
}
