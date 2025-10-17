using BetterAmongUs.Helpers;
using HarmonyLib;

namespace BetterAmongUs.Patches.Gameplay;

class RolePatch
{
    [HarmonyPatch(typeof(NoisemakerRole))]
    class NoisemakerRolePatch
    {
        [HarmonyPatch(nameof(NoisemakerRole.OnDeath))]
        [HarmonyPrefix]
        internal static bool NotifyOfDeath_Prefix(NoisemakerRole __instance)
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
