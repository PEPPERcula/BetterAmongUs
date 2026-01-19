using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches.Gameplay.UI;

internal static class MiniMapBehaviourPatch
{
    [HarmonyPatch(typeof(MapBehaviour))]
    internal static class MapBehaviourPatch
    {
        [HarmonyPatch(nameof(MapBehaviour.ShowNormalMap))]
        [HarmonyPatch(nameof(MapBehaviour.ShowDetectiveMap))]
        [HarmonyPostfix]
        private static void ShowMap_Postfix(MapBehaviour __instance) => __instance.ColorControl.SetColor(new Color(0.05f, 0.6f, 1f, 1f));
        [HarmonyPatch(nameof(MapBehaviour.ShowSabotageMap))]
        [HarmonyPostfix]
        private static void ShowSabotageMap_Postfix(MapBehaviour __instance) => __instance.ColorControl.SetColor(new Color(1f, 0.3f, 0f, 1f));
        [HarmonyPatch(nameof(MapBehaviour.ShowCountOverlay))]
        [HarmonyPostfix]
        private static void ShowCountOverlay_Postfix(MapBehaviour __instance) => __instance.ColorControl.SetColor(new Color(0.2f, 0.5f, 0f, 1f));
    }

    [HarmonyPatch(typeof(MapConsole))]
    internal static class MapConsolePatch
    {
        [HarmonyPatch(nameof(MapConsole.Use))]
        [HarmonyPostfix]
        private static void ShowCountOverlay_Postfix() => MapBehaviour.Instance.ColorControl.SetColor(new Color(0.2f, 0.5f, 0f, 1f));
    }
}
