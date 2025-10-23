using HarmonyLib;

namespace BetterAmongUs.Patches.Managers;

[HarmonyPatch(typeof(LoadingBarManager))]
internal static class LoadingBarManagerPatch
{
    [HarmonyPatch(nameof(LoadingBarManager.SetLoadingPercent))]
    [HarmonyPrefix]
    private static bool SetLoadingPercent_Prefix()
    {
        return false;
    }

    [HarmonyPatch(nameof(LoadingBarManager.ToggleLoadingBar))]
    [HarmonyPrefix]
    private static bool ToggleLoadingBart_Prefix()
    {
        return false;
    }
}
