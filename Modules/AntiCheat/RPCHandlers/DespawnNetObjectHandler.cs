using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat;

public class DespawnNetObjectHandler : RPCHandler
{
    public override byte GameDataTag => (byte)HandleGameDataTags.NetObjectDespawn;

    public override void HandleGameData(MessageReader reader)
    {
        if (!GameState.IsHost) return;

        uint netId = reader.ReadPackedUInt32();
        var innerNetObject = innerNetClient.FindObjectByNetId<InnerNetObject>(netId);
        if (innerNetObject is PlayerControl player)
        {
            BetterNotificationManager.NotifyCheat(player, "Attempting to despawn player", kickPlayer: false);
            player.Kick(true);
        }
    }
}