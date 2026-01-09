using BetterAmongUs.Mono;
using HarmonyLib;

namespace BetterAmongUs.Patches.Gameplay;

internal static class RolePatch
{
    [HarmonyPatch(typeof(NoisemakerRole))]
    internal static class NoisemakerRolePatch
    {
        [HarmonyPatch(nameof(NoisemakerRole.OnDeath))]
        [HarmonyPrefix]
        private static bool NotifyOfDeath_Prefix(NoisemakerRole __instance)
        {
            if (__instance.Player.BetterData().RoleInfo.HasNoisemakerNotify)
            {
                return false;
            }

            __instance.Player.BetterData().RoleInfo.HasNoisemakerNotify = true;

            return true;
        }
    }
}
