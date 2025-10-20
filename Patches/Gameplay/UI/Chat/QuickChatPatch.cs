using AmongUs.QuickChat;
using BetterAmongUs.Helpers;
using HarmonyLib;

namespace BetterAmongUs.Patches.Gameplay.UI.Chat;

[HarmonyPatch]
internal static class QuickChatPatch
{
    [HarmonyPatch(typeof(QuickChatMenu))]
    internal static class QuickChatMenuPatch
    {
        [HarmonyPatch(nameof(QuickChatMenu.Awake))]
        [HarmonyPrefix]
        private static void Awake_Prefix(QuickChatMenu __instance)
        {
            __instance.closeButton?.gameObject?.SetUIColors("Icon");
        }
    }

    [HarmonyPatch(typeof(QuickChatMenuLandingPage))]
    internal static class QuickChatMenuLandingPagePatch
    {
        [HarmonyPatch(nameof(QuickChatMenuLandingPage.Initialize))]
        [HarmonyPrefix]
        private static void Initialize_Prefix(QuickChatMenuLandingPage __instance)
        {
            __instance.buttonTemplate?.Button?.gameObject?.SetUIColors("Icon");
            __instance.favoritesButton?.Button?.gameObject?.SetUIColors("Icon");
            __instance.remarksButton?.Button?.gameObject?.SetUIColors("Icon");
        }
    }

    [HarmonyPatch(typeof(QuickChatMenuPhrasesPage))]
    internal static class QuickChatMenuPhrasesPagePatch
    {
        [HarmonyPatch(nameof(QuickChatMenuPhrasesPage.Awake))]
        [HarmonyPrefix]
        private static void Awake_Prefix(QuickChatMenuPhrasesPage __instance)
        {
            __instance.crewmateButtonTemplate?.Button?.gameObject?.SetUIColors("Icon", "Background", "PlayerMask", "Skin", "Visor", "Back", "Front", "Normal", "Horse", "Seeker",
                "LongBoiBody", "LongHead", "LongNeck", "ForegroundNeck", "LongHands");
            __instance.phraseButtonTemplate?.Button?.gameObject?.SetUIColors("Icon");
        }
    }
}
