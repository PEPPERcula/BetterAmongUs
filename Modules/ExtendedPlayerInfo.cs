namespace BetterAmongUs;

public class ExtendedPlayerInfo
{
    public bool IsBetterUser { get; set; } = false;
    public bool IsBetterHost { get; set; } = false;
    public bool IsTOHEHost { get; set; } = false;
    public bool HasNoisemakerNotify { get; set; } = false;
    public float TimeSinceKill { get; set; } = 0f;
    public int TimesCalledMeeting { get; set; } = 0;

}

public static class PlayerControlExtensions
{
    private static readonly Dictionary<PlayerControl, ExtendedPlayerInfo> playerInfo = [];

    public static ExtendedPlayerInfo BetterData(this PlayerControl player)
    {
        if (!playerInfo.ContainsKey(player))
        {
            playerInfo[player] = new ExtendedPlayerInfo();
        }

        return playerInfo[player];
    }
}
