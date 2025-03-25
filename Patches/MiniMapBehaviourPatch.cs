using HarmonyLib;
using UnityEngine;


namespace BetterAmongUs.Patches;

internal class MiniMapBehaviourPatch
{
    [HarmonyPatch(typeof(MapBehaviour))]
    class MapBehaviourPatch
    {
        [HarmonyPatch(nameof(MapBehaviour.ShowNormalMap))]
        [HarmonyPostfix]
        internal static void ShowNormalMap_Postfix(MapBehaviour __instance) => __instance.ColorControl.SetColor(new Color(0.05f, 0.6f, 1f, 1f));
        [HarmonyPatch(nameof(MapBehaviour.ShowSabotageMap))]
        [HarmonyPostfix]
        internal static void ShowSabotageMap_Postfix(MapBehaviour __instance) => __instance.ColorControl.SetColor(new Color(1f, 0.3f, 0f, 1f));
        [HarmonyPatch(nameof(MapBehaviour.ShowCountOverlay))]
        [HarmonyPostfix]
        internal static void ShowCountOverlay_Postfix(MapBehaviour __instance) => __instance.ColorControl.SetColor(new Color(0.2f, 0.5f, 0f, 1f));
    }

    [HarmonyPatch(typeof(MapConsole))]
    class MapConsolePatch
    {
        [HarmonyPatch(nameof(MapConsole.Use))]
        [HarmonyPostfix]
        internal static void ShowCountOverlay_Postfix() => MapBehaviour.Instance.ColorControl.SetColor(new Color(0.2f, 0.5f, 0f, 1f));
    }
}
