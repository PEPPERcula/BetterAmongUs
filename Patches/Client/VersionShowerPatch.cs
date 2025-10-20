using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using HarmonyLib;

namespace BetterAmongUs.Patches.Client;

[HarmonyPatch(typeof(VersionShower))]
internal static class VersionShowerPatch
{
    [HarmonyPatch(nameof(VersionShower.Start))]
    [HarmonyPostfix]
    private static void Postfix(VersionShower __instance)
    {
        string mark = Translator.GetString("BAUMark");
        string bau = Translator.GetString("BAU");
        __instance.text.text = $"<color=#0dff00>{mark}{bau}{mark} {BAUPlugin.GetVersionText()}</color> <color=#ababab>~</color> {Utils.GetPlatformName(BAUPlugin.PlatformData.Platform)} v{BAUPlugin.AmongUsVersion} ({BAUPlugin.AppVersion})";
    }
}
