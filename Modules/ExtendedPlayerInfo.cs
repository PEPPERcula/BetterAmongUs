
namespace BetterAmongUs;

static class ExtendedPlayerInfo
{
    private static Dictionary<PlayerControl, bool> IsBetterUser = new Dictionary<PlayerControl, bool>();

    public static bool GetIsBetterUser(this PlayerControl player)
    {
        if (IsBetterUser.ContainsKey(player))
            return IsBetterUser[player];
        else
            return false;
    }

    public static void SetIsBetterUser(this PlayerControl player, bool value)
    {
        IsBetterUser[player] = value;
    }
}

