using BetterAmongUs.Helpers;
using HarmonyLib;

namespace BetterAmongUs.Patches.Gameplay.UI;

[HarmonyPatch(typeof(KillOverlay))]
internal static class KillOverlayPatch
{
    [HarmonyPatch(nameof(KillOverlay.ShowKillAnimation), [typeof(OverlayKillAnimation), typeof(NetworkedPlayerInfo), typeof(NetworkedPlayerInfo)])]
    [HarmonyPrefix]
    private static bool ShowKillAnimation_Prefix()
    {
        if (!PlayerControl.LocalPlayer.IsAlive())
        {
            return false;
        }

        return true;
    }
}
