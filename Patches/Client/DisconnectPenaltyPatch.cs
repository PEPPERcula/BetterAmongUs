using AmongUs.Data.Player;
using HarmonyLib;

namespace BetterAmongUs.Patches.Client;

[HarmonyPatch(typeof(PlayerBanData))]
internal static class DisconnectPenaltyPatch
{
    [HarmonyPatch(nameof(PlayerBanData.IsBanned), MethodType.Getter)]
    [HarmonyPrefix]
    private static bool IsBanned_Prefix(PlayerBanData __instance, ref bool __result)
    {
        __instance.BanPoints = 0f;
        __instance.banPoints = 0f;
        __result = false;
        return false;
    }
}
