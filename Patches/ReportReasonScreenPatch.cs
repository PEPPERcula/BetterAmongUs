using HarmonyLib;

namespace BetterAmongUs.Patches;

[HarmonyPatch(typeof(ReportReasonScreen))]
class ReportReasonScreenPatch
{
    [HarmonyPatch(nameof(ReportReasonScreen.Show))]
    [HarmonyPrefix]
    public static void Show_Prefix(ref string playerName)
    {
        if (playerName.Contains("Anti-Cheat"))
        {
            string extractedText = playerName.Split(new[] { "<color=#ffea00>", "</color>" }, StringSplitOptions.None)[1];
            playerName = extractedText;
        }

    }
}
