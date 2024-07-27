using HarmonyLib;
using UnityEngine;


namespace BetterAmongUs.Patches;

public class MiniMapBehaviourPatch
{
    [HarmonyPatch(typeof(MapBehaviour))]
    class MapBehaviourPatch
    {
        [HarmonyPatch(nameof(MapBehaviour.Awake))]
        [HarmonyPostfix]
        public static void Awake_Postfix(MapBehaviour __instance)
        {
            PoolablePlayer NewDisplay = __instance.HerePoint.gameObject.AddComponent<PoolablePlayer>();
            NewDisplay.SetBodyCosmeticsVisible(true);
        }
        [HarmonyPatch(nameof(MapBehaviour.Show))]
        [HarmonyPrefix]
        public static bool Show_Prefix(MapBehaviour __instance, ref MapOptions opts)
        {
            if (__instance.IsOpen)
            {
                __instance.Close();
                return false;
            }
            if (!PlayerControl.LocalPlayer.CanMove && !PlayerControl.LocalPlayer.IsInVent() && !PlayerControl.LocalPlayer.onLadder && !PlayerControl.LocalPlayer.inMovingPlat && !GameStates.IsMeeting)
            {
                return false;
            }
            switch (opts.Mode)
            {
                case MapOptions.Modes.Normal:
                    __instance.ShowNormalMap();
                    break;
                case MapOptions.Modes.CountOverlay:
                    __instance.ShowCountOverlay(opts.AllowMovementWhileMapOpen, opts.ShowLivePlayerPosition, opts.IncludeDeadBodies);
                    break;
                case MapOptions.Modes.Sabotage:
                    __instance.ShowSabotageMap();
                    break;
            }

            return false;
        }
        [HarmonyPatch(nameof(MapBehaviour.ShowNormalMap))]
        [HarmonyPrefix]
        public static bool ShowNormalMap_Prefix(MapBehaviour __instance)
        {
            if (__instance.IsOpen)
            {
                __instance.Close();
                return false;
            }
            if (!PlayerControl.LocalPlayer.CanMove && !PlayerControl.LocalPlayer.IsInVent() && !PlayerControl.LocalPlayer.onLadder && !PlayerControl.LocalPlayer.inMovingPlat && !GameStates.IsMeeting)
            {
                return false;
            }
            PlayerControl.LocalPlayer.SetPlayerMaterialColors(__instance.HerePoint);
            if (PlayerControl.LocalPlayer.isTrackingPlayer)
            {
                if (PlayerControl.LocalPlayer.trackedPlayer.Data.Disconnected)
                {
                    __instance.TrackedHerePoint.gameObject.SetActive(false);
                }
                else
                {
                    __instance.SetTrackedHerePointColor(PlayerControl.LocalPlayer.trackedPlayerColorID);
                    __instance.TrackedHerePoint.gameObject.SetActive(true);
                    __instance.UpdateTrackedPosition();
                }
            }
            __instance.GenericShow();
            __instance.taskOverlay.Show();
            __instance.ColorControl.SetColor(new Color(0.05f, 0.6f, 1f, 1f));
            DestroyableSingleton<HudManager>.Instance.SetHudActive(false);
            return false;
        }
        [HarmonyPatch(nameof(MapBehaviour.ShowSabotageMap))]
        [HarmonyPrefix]
        public static bool ShowSabotageMap_Prefix(MapBehaviour __instance)
        {
            if (!PlayerControl.LocalPlayer.CanMove && !PlayerControl.LocalPlayer.IsInVent() && !PlayerControl.LocalPlayer.onLadder && !PlayerControl.LocalPlayer.inMovingPlat)
            {
                return false;
            }
            PlayerControl.LocalPlayer.SetPlayerMaterialColors(__instance.HerePoint);
            __instance.GenericShow();
            __instance.infectedOverlay.gameObject.SetActive(true);
            __instance.taskOverlay.Show();
            __instance.ColorControl.SetColor(new Color(1f, 0.3f, 0f, 1f));
            return false;
        }
        [HarmonyPatch(nameof(MapBehaviour.ShowCountOverlay))]
        [HarmonyPostfix]
        public static void ShowCountOverlay_Postfix(MapBehaviour __instance) => __instance.ColorControl.SetColor(new Color(0.2f, 0.5f, 0f, 1f));
    }

    [HarmonyPatch(typeof(MapConsole))]
    class MapConsolePatch
    {
        [HarmonyPatch(nameof(MapConsole.Use))]
        [HarmonyPostfix]
        public static void ShowCountOverlay_Postfix() => MapBehaviour.Instance.ColorControl.SetColor(new Color(0.2f, 0.5f, 0f, 1f));
    }
}
