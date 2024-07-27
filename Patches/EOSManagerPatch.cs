using HarmonyLib;

namespace BetterAmongUs.Patches;

[HarmonyPatch(typeof(EOSManager))]
public class EOSManagerPatch
{
    [HarmonyPatch(nameof(EOSManager.Update))]
    [HarmonyPostfix]
    public static void Postfix(EOSManager __instance)
    {
        __instance.ageOfConsent = 21;
        __instance.isKWSMinor = false;
    }
}
