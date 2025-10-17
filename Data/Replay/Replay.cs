using BetterAmongUs.Data.Replay.Events;

namespace BetterAmongUs.Data.Replay;

[Serializable]
internal class Replay
{
    public int MapId;
    public List<PlayerReplayData> PlayerData = [];
    public (float timeStamp, IReplayEvent) Events;

    public void Load()
    {
        AmongUsClient.Instance.TutorialMapId = MapId;
        UnityEngine.Object.FindFirstObjectByType<FreeplayPopover>().hostGameButton.OnClick();
        CreatePlayers();
    }

    private void CreatePlayers()
    {

    }

    public void UpdateMovement(float timeStamp)
    {

    }

    public void UpdateEvents(float timeStamp)
    {

    }
}
