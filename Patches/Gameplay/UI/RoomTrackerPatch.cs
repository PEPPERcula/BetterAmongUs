using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches.Gameplay.UI;

[HarmonyPatch(typeof(RoomTracker))]
internal static class RoomTrackerPatch
{
    [HarmonyPatch(nameof(RoomTracker.Awake))]
    [HarmonyPostfix]
    private static void Awake_Postfix(RoomTracker __instance)
    {
        var aspectPosition = __instance.gameObject.AddComponent<AspectPosition>();
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.Bottom;
        aspectPosition.DistanceFromEdge = new Vector3(0f, 0.3f, 0f);
        aspectPosition.updateAlways = true;
    }
}
