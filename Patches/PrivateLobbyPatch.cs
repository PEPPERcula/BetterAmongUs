using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Patches;

[HarmonyPatch]
internal class PrivateLobbyPatch
{
    private static AprilFoolsModeToggleButton? toggle;
    private static TextMeshPro? toggleText;

    [HarmonyPatch(typeof(CreateGameOptions))]
    [HarmonyPatch(nameof(CreateGameOptions.Show))]
    [HarmonyPostfix]
    internal static void CreateGameOptionsShow_Postfix(CreateGameOptions __instance)
    {
        if (toggle != null) return;

        toggle = UnityEngine.Object.Instantiate(__instance.AprilFoolsToggle, __instance.transform);
        if (toggle != null)
        {
            toggle.gameObject.SetActive(true);
            var onButton = toggle.onButton.GetComponent<PassiveButton>();
            if (onButton != null)
            {
                onButton.OnClick = new();
                onButton.OnClick.AddListener((Action)(() => TogglePrivateOnlyLobby(true)));
            }
            var offButton = toggle.offButton.GetComponent<PassiveButton>();
            if (offButton != null)
            {
                offButton.OnClick = new();
                offButton.OnClick.AddListener((Action)(() => TogglePrivateOnlyLobby(false)));
            }
            var aspect = toggle.gameObject.AddComponent<AspectPosition>();
            aspect.Alignment = AspectPosition.EdgeAlignments.Top;
            aspect.DistanceFromEdge = new UnityEngine.Vector3(0, 1.7f, 0);
            aspect.AdjustPosition();

            var text = toggle.transform.Find("AprilFoolsModeText")?.GetComponent<TextMeshPro>();
            if (text != null)
            {
                text.DestroyTextTranslator();
                text.text = "Private Only Lobby";
                toggleText = text;
            }

            var banner = toggle.transform.Find("Banner")?.GetComponent<SpriteRenderer>();
            if (banner != null)
            {
                banner.color = new(1, 1, 1, 0.25f);
            }
        }
    }

    private static void TogglePrivateOnlyLobby(bool modeOn)
    {
        Main.PrivateOnlyLobby.Value = modeOn;
    }

    [HarmonyPatch(typeof(AprilFoolsModeToggleButton))]
    [HarmonyPatch(nameof(AprilFoolsModeToggleButton.Update))]
    [HarmonyPrefix]
    internal static bool AprilFoolsModeToggleButtonUpdate_Prefix(AprilFoolsModeToggleButton __instance)
    {
        if (__instance == toggle)
        {
            if (Main.PrivateOnlyLobby.Value)
            {
                __instance.offButton.OutColor = Color.grey;
                __instance.offText.color = Color.grey;
                __instance.offButtonSprite.color = Color.grey;
                __instance.onButton.OutColor = Color.white;
                __instance.onText.color = Color.white;
                __instance.onButtonSprite.color = Color.green;
                if (toggleText != null) toggleText.color = new(1, 1, 1, 1);
            }
            else
            {
                __instance.onButton.OutColor = Color.grey;
                __instance.onText.color = Color.grey;
                __instance.onButtonSprite.color = Color.grey;
                __instance.offButton.OutColor = Color.white;
                __instance.offText.color = Color.white;
                __instance.offButtonSprite.color = Color.green;
                if (toggleText != null) toggleText.color = new(1, 1, 1, 0.5f);
            }

            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(LobbyInfoPane))]
    [HarmonyPatch(nameof(LobbyInfoPane.Update))]
    [HarmonyPostfix]
    internal static void LobbyInfoPaneUpdate_Postfix(LobbyInfoPane __instance)
    {
        if (Main.PrivateOnlyLobby.Value && !GameState.IsLocalGame && GameState.IsHost)
        {
            if (AmongUsClient.Instance.IsGamePublic)
            {
                AmongUsClient.Instance.ChangeGamePublic(false);
            }

            var button = __instance.HostPrivateButton.GetComponent<PassiveButton>();
            if (button != null)
            {
                button.DestroyMono();

                var sprite = __instance.HostPrivateButton.transform.Find("Inactive").GetComponent<SpriteRenderer>();
                if (sprite != null)
                {
                    sprite.color = new(0.35f, 1, 1, 1);
                }
            }
        }
    }
}
