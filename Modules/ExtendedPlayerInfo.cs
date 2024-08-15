using AmongUs.GameOptions;
using InnerNet;

namespace BetterAmongUs;

public class ExtendedPlayerInfo
{
    public Dictionary<byte, string> LastNameSetFor { get; set; } = [];
    public bool IsBetterUser { get; set; } = false;
    public bool IsBetterHost { get; set; } = false;
    public bool IsTOHEHost { get; set; } = false;
    public bool BannedByAntiCheat { get; set; } = false;
    public bool HasNoisemakerNotify { get; set; } = false;
    public int TimesCalledMeeting { get; set; } = 0;
    public RoleTypes DeadDisplayRole { get; set; }
}

public static class PlayerControlExtensions
{
    private static readonly Dictionary<NetworkedPlayerInfo, ExtendedPlayerInfo> playerInfo = [];

    // Get BetterData from PlayerControl
    public static ExtendedPlayerInfo BetterData(this PlayerControl player)
    {
        if (!playerInfo.ContainsKey(player.Data))
        {
            playerInfo[player.Data] = new ExtendedPlayerInfo();
        }

        return playerInfo[player.Data];
    }

    // Get BetterData from NetworkedPlayerInfo
    public static ExtendedPlayerInfo BetterData(this NetworkedPlayerInfo info)
    {
        if (!playerInfo.ContainsKey(info))
        {
            playerInfo[info] = new ExtendedPlayerInfo();
        }

        return playerInfo[info];
    }

    // Get BetterData from ClientData
    public static ExtendedPlayerInfo? BetterData(this ClientData data)
    {
        var player = Utils.PlayerFromClientId(data.Id);

        if (player != null)
        {
            if (!playerInfo.ContainsKey(player.Data))
            {
                playerInfo[player.Data] = new ExtendedPlayerInfo();
            }

            return playerInfo[player.Data];
        }

        return null;
    }
}
