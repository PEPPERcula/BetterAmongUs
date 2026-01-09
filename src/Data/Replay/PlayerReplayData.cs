using AmongUs.GameOptions;
using UnityEngine;

namespace BetterAmongUs.Data.Replay;

[Serializable]
internal class PlayerReplayData
{
    public int PlayerId;
    public string PlayerName = "";
    public RoleTypes Role;
    public (float timeStamp, Vector2 pos)[] MovementDataBuffer = [];
    public (int colorId, string skinId, string visorId, string petId, string namePlateId) CosmeticData = new();
}
