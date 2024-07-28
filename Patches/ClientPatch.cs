using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches;

public class ClientPatch
{
    [HarmonyPatch(typeof(AmongUsClient))]
    public class AmongUsClientPatch
    {
        [HarmonyPatch(nameof(AmongUsClient.ExitGame))]
        [HarmonyPostfix]
        public static void ExitGame_Postfix([HarmonyArgument(0)] DisconnectReasons reason)
        {
            Logger.Log($"Client has left game for: {Enum.GetName(reason)}", "AmongUsClientPatch");
        }
    }
    [HarmonyPatch(typeof(CosmeticsLayer))]
    public class CosmeticsLayerPatch
    {
        [HarmonyPatch(nameof(CosmeticsLayer.SetColorBlindColor))]
        [HarmonyPrefix]
        public static bool SetColorBlindColor_Prefix(CosmeticsLayer __instance, [HarmonyArgument(0)] int color)
        {
            if (__instance.colorBlindText == null || !__instance.showColorBlindText)
            {
                return false;
            }
            __instance.colorBlindText.text = __instance.GetColorBlindText();
            __instance.colorBlindText.color = Palette.PlayerColors[color];
            __instance.colorBlindText.transform.localPosition = new Vector3(0f, -1.5f, 0.4999f);
            return false;
        }
    }
}
