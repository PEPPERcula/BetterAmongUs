
namespace BetterAmongUs;

static class ExtendedPlayerInfo
{
    public static List<PlayerControl> HasNoisemakerNotify = [];
    public static Dictionary<PlayerControl, float> TimeSinceKill = [];
    public static Dictionary<PlayerControl, int> TimesCalledMeeting = [];
    private static Dictionary<PlayerControl, bool> IsBetterUser = new Dictionary<PlayerControl, bool>();
    private static Dictionary<PlayerControl, bool> IsBetterHost = new Dictionary<PlayerControl, bool>();

    public static bool GetIsBetterUser(this PlayerControl player)
    {
        if (IsBetterUser.ContainsKey(player))
            return IsBetterUser[player];
        else
            return false;
    }

    public static bool GetIsBetterHost(this PlayerControl player)
    {
        if (player.IsHost() && IsBetterHost.ContainsKey(player))
            return IsBetterHost[player];
        else
            return false;
    }

    public static void SetIsBetterUser(this PlayerControl player, bool value)
    {
        IsBetterUser[player] = value;
    }

    public static void SetIsBetterHost(this PlayerControl player, bool value)
    {
        IsBetterHost[player] = value;
    }
}

