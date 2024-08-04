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
            if (ExtendedPlayerInfo.HasNoisemakerNotify.Contains(__instance.Player))
            {
                return false;
            }

            ExtendedPlayerInfo.HasNoisemakerNotify.Add(__instance.Player);

            return true;
        }
    }
}
