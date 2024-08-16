using HarmonyLib;

namespace BetterAmongUs.Patches;

class RolePatch
{
    [HarmonyPatch(typeof(NoisemakerRole))]
    class NoisemakerRolePatch
    {
        [HarmonyPatch(nameof(NoisemakerRole.OnDeath))]
        [HarmonyPrefix]
        public static bool NotifyOfDeath_Prefix(NoisemakerRole __instance)
        {
            if (__instance.Player.BetterData().HasNoisemakerNotify)
            {
                return false;
            }

            __instance.Player.BetterData().HasNoisemakerNotify = true;

            return true;
        }
    }
}
