using HarmonyLib;

namespace BetterAmongUs.Patches;

internal class ButtonsPatch
{
    // Handle EFC
    [HarmonyPatch(typeof(SabotageButton))]
    internal class SabotageButtonPatch
    {
        [HarmonyPatch(nameof(SabotageButton.DoClick))]
        [HarmonyPrefix]
        public static bool DoClick_Prefix(SabotageButton __instance)
        {
            if (PlayerControl.LocalPlayer.Data.Role.IsImpostor && GameManager.Instance.SabotagesEnabled())
            {
                DestroyableSingleton<HudManager>.Instance.ToggleMapVisible(new MapOptions
                {
                    Mode = MapOptions.Modes.Sabotage
                });
            }

            return false;
        }
        [HarmonyPatch(nameof(SabotageButton.Refresh))]
        [HarmonyPrefix]
        public static bool Refresh_Prefix(SabotageButton __instance)
        {
            if (GameManager.Instance == null || PlayerControl.LocalPlayer == null)
            {
                __instance.ToggleVisible(false);
                __instance.SetDisabled();
                return false;
            }
            if (!GameManager.Instance.SabotagesEnabled() || PlayerControl.LocalPlayer.petting)
            {
                __instance.ToggleVisible(PlayerControl.LocalPlayer.Data.Role.IsImpostor && GameManager.Instance.SabotagesEnabled());
                __instance.SetDisabled();
                return false;
            }
            __instance.SetEnabled();

            return false;
        }
    }
}
