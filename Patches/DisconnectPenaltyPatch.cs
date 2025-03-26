using AmongUs.Data.Player;
using HarmonyLib;

namespace BetterAmongUs.Patches;

[HarmonyPatch(typeof(PlayerBanData), nameof(PlayerBanData.BanMinutesLeft), MethodType.Getter)]
internal static class DisconnectPenaltyPatch
{
    internal static bool Prefix(PlayerBanData __instance, ref int __result)
    {
        if (__instance.BanPoints != 0f)
        {
            __instance.BanPoints = 0f;
            __result = 0;
            return false;
        }
        return true;
    }
}
