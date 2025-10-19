using BetterAmongUs.Helpers;
using HarmonyLib;

namespace BetterAmongUs.Patches.Gameplay.UI;

[HarmonyPatch(typeof(ReportReasonScreen))]
class ReportReasonScreenPatch
{
    [HarmonyPatch(nameof(ReportReasonScreen.Show))]
    [HarmonyPrefix]
    internal static void Show_Prefix(ref string playerName)
    {
        if (Utils.IsHtmlText(playerName))
        {
            string extractedText = playerName.Split(["<color=#ffea00>", "</color>"], StringSplitOptions.None)[1];
            playerName = extractedText;
        }
    }
}
