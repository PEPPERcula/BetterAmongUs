using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using HarmonyLib;

namespace BetterAmongUs.Patches.Client;

[HarmonyPatch(typeof(VersionShower))]
internal class VersionShowerPatch
{
    [HarmonyPatch(nameof(VersionShower.Start))]
    [HarmonyPostfix]
    internal static void Postfix(VersionShower __instance)
    {
        string mark = Translator.GetString("BAUMark");
        string bau = Translator.GetString("BAU");
        __instance.text.text = $"<color=#0dff00>{mark}{bau}{mark} {Main.GetVersionText()}</color> <color=#ababab>~</color> {Utils.GetPlatformName(Main.PlatformData.Platform)} v{Main.AmongUsVersion} ({Main.AppVersion})";
    }
}
