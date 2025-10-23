using BetterAmongUs.Managers;
using BetterAmongUs.Network;
using BetterAmongUs.Patches.Gameplay.UI;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat;

internal class DeserializeNetObjectHandler : RPCHandler
{
    internal override byte GameDataTag => (byte)HandleGameDataTags.NetObjectDeserialize;

    internal override void HandleGameData(MessageReader reader)
    {
        uint netId = reader.ReadPackedUInt32();
        var innerNetObject = innerNetClient.FindObjectByNetId<InnerNetObject>(netId);
        if (innerNetObject?.TryCast<CustomNetworkTransform>() && (GameState.IsMeeting && MeetingHudPatch.timeOpen > 5))
        {
            var player = innerNetObject.Cast<CustomNetworkTransform>()?.myPlayer;
            if (player == null) return;
            if (BetterNotificationManager.NotifyCheat(player, "Attempting to move in meeting", forceBan: true))
            {
                LogRpcInfo($"{innerNetObject.TryCast<CustomNetworkTransform>() is CustomNetworkTransform} && {GameState.IsMeeting} && {MeetingHud.Instance.state != MeetingHud.VoteStates.Animating}", player);
            }
        }
    }
}