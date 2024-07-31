
using HarmonyLib;

// Enable modded protocol
namespace BetterAmongUs.Patches;

[HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
class ServerUpdatePatch
{
    static void Postfix(ref int __result)
    {
        if (GameStates.IsLocalGame)
        {
            Logger.Log($"IsLocalGame: {__result}", "VersionServer");
        }
        if (GameStates.IsOnlineGame)
        {
            // __result += 25;
            Logger.Log($"IsOnlineGame: {__result}", "VersionServer");
        }
    }
}
[HarmonyPatch(typeof(Constants), nameof(Constants.IsVersionModded))]
public static class IsVersionModdedPatch
{
    public static bool Prefix(ref bool __result)
    {
        __result = true;
        return false;
    }
}