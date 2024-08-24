using HarmonyLib;

namespace BetterAmongUs.Patches;

[HarmonyPatch(typeof(VersionShower))]
public class VersionShowerPatch
{
    [HarmonyPatch(nameof(VersionShower.Start))]
    [HarmonyPostfix]
    public static void Postfix(VersionShower __instance)
    {
        __instance.text.text = $"<color=#0dff00>♻BetterAmongUs♻ {Main.GetVersionText()}</color> - " + __instance.text.text;
    }
}
