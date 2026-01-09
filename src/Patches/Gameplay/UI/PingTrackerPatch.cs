using BetterAmongUs.Mono;
using HarmonyLib;

namespace BetterAmongUs.Patches.Gameplay.UI;

[HarmonyPatch(typeof(PingTracker))]
internal static class PingTrackerPatch
{
    [HarmonyPatch(nameof(PingTracker.Update))]
    [HarmonyPrefix]
    private static bool Prefix(PingTracker __instance)
    {
        var betterPingTracker = __instance.gameObject.AddComponent<BetterPingTracker>();
        betterPingTracker.SetUp(__instance.text, __instance.aspectPosition);
        __instance.enabled = false;

        return false;
    }
}
