using BetterAmongUs.Helpers;
using HarmonyLib;

namespace BetterAmongUs.Patches.Gameplay.Player;

// Set text color
[HarmonyPatch(typeof(CosmeticsLayer))]
internal static class CosmeticsLayerPatch
{
    [HarmonyPatch(nameof(CosmeticsLayer.GetColorBlindText))]
    [HarmonyPrefix]
    private static bool GetColorBlindText_Prefix(CosmeticsLayer __instance, ref string __result)
    {
        if (__instance.bodyMatProperties.ColorId > Palette.PlayerColors.Length) return true;

        string colorName = Palette.GetColorName(__instance.bodyMatProperties.ColorId);

        if (!string.IsNullOrEmpty(colorName))
        {
            __result = (char.ToUpperInvariant(colorName[0]) + colorName.Substring(1).ToLowerInvariant()).ToColor(Palette.PlayerColors[__instance.bodyMatProperties.ColorId]);
        }
        else
        {
            __result = string.Empty;
        }

        return false;
    }
}
