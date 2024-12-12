using BetterAmongUs.Managers;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat;

public class DeserializeNetObjectHandler : RPCHandler
{
    public override byte GameDataTag => (byte)HandleGameDataTags.NetObjectDeserialize;

    public override void HandleGameData(MessageReader reader)
    {
        uint netId = reader.ReadPackedUInt32();
        var innerNetObject = innerNetClient.FindObjectByNetId<InnerNetObject>(netId);
        if (innerNetObject?.TryCast<CustomNetworkTransform>() && GameState.IsMeeting && MeetingHudPatch.timeOpen >= 5f)
        {
            var player = innerNetObject.Cast<CustomNetworkTransform>()?.myPlayer;
            if (player == null) return;
            BetterNotificationManager.NotifyCheat(player, "Attempting to move in meeting", forceBan: true);
            LogRpcInfo($"{innerNetObject.TryCast<CustomNetworkTransform>()} && {GameState.IsMeeting} && {MeetingHudPatch.timeOpen >= 5f}", player);
        }
    }
}