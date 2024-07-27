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
            if (__instance.Player == null || !__instance.Player.IsAlive())
            {
                return false;
            }

            return true;
        }
    }
}
