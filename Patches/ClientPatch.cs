using HarmonyLib;
using InnerNet;
using UnityEngine;

namespace BetterAmongUs.Patches;

public class ClientPatch
{
    // Log game exit
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
    [HarmonyPatch(typeof(InnerNetClient))]
    public class InnerNetClientPatch
    {
        [HarmonyPatch(nameof(InnerNetClient.CanBan))]
        [HarmonyPrefix]
        public static bool CanBan_Prefix(ref bool __result)
        {
            __result = GameStates.IsHost;
            return false;
        }
        [HarmonyPatch(nameof(InnerNetClient.CanKick))]
        [HarmonyPrefix]
        public static bool CanKick_Prefix(ref bool __result)
        {
            __result = GameStates.IsHost || (GameStates.IsInGamePlay && (GameStates.IsMeeting || GameStates.IsExilling));
            return false;
        }
    }
    // Set text color
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
    // Clean up menu
    [HarmonyPatch(typeof(RegionMenu))]
    public class RegionMenuPatch
    {
        [HarmonyPatch(nameof(RegionMenu.OnEnable))]
        [HarmonyPostfix]
        public static void AdjustButtonPositions_Postfix(RegionMenu __instance)
        {
            const int buttonsPerColumn = 6;
            float buttonSpacing = 0.6f;
            float buttonSpacingSide = 2.25f;

            List<UiElement> buttons = __instance.controllerSelectable.ToArray().ToList();

            int columnCount = (buttons.Count + buttonsPerColumn - 1) / buttonsPerColumn;
            float totalWidth = (columnCount - 1) * buttonSpacingSide;
            float totalHeight = (buttonsPerColumn - 1) * buttonSpacing;

            Vector3 startPosition = new Vector3(-totalWidth / 2, totalHeight / 2, 0f);

            for (int i = 0; i < buttons.Count; i++)
            {
                int col = i / buttonsPerColumn;
                int row = i % buttonsPerColumn;
                buttons[i].transform.localPosition = startPosition + new Vector3(col * buttonSpacingSide, -row * buttonSpacing, 0f);
            }
        }
    }
}
