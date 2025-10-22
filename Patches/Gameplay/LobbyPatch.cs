using BetterAmongUs.Helpers;
using BetterAmongUs.Items.OptionItems;
using BetterAmongUs.Modules;
using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches.Gameplay;

[HarmonyPatch]
internal static class LobbyPatch
{
    [HarmonyPatch(typeof(LobbyBehaviour))]
    internal static class LobbyBehaviourPatch
    {
        [HarmonyPatch(nameof(LobbyBehaviour.Start))]
        [HarmonyPostfix]
        private static void Start_Postfix(/*LobbyBehaviour __instance*/)
        {
            OptionPlayerItem.ResetAllValues();
        }

        // Disabled annoying music
        [HarmonyPatch(nameof(LobbyBehaviour.Update))]
        [HarmonyPostfix]
        private static void Update_Postfix(/*LobbyBehaviour __instance*/)
        {
            if (BAUPlugin.DisableLobbyTheme.Value)
                SoundManager.instance.StopSound(LobbyBehaviour.Instance.MapTheme);
        }

        [HarmonyPatch(nameof(LobbyBehaviour.RpcExtendLobbyTimer))]
        [HarmonyPostfix]
        private static void RpcExtendLobbyTimer_Postfix(/*LobbyBehaviour __instance*/)
        {
            GameStartManagerPatch.lobbyTimer += 30f;
        }
    }

    [HarmonyPatch(typeof(LobbyViewSettingsPane))]
    internal static class LobbyViewSettingsPanePatch
    {
        [HarmonyPatch(nameof(LobbyViewSettingsPane.Awake))]
        [HarmonyPostfix]
        private static void Awake_Postfix(LobbyViewSettingsPane __instance)
        {
            __instance.backButton.gameObject.SetUIColors("Icon");
            __instance.taskTabButton.gameObject.SetUIColors("Icon");
            __instance.rolesTabButton.gameObject.SetUIColors("Icon");
        }
    }

    [HarmonyPatch(typeof(GameStartManager))]
    internal static class GameStartManagerPatch
    {
        internal static float lobbyTimer = 600f;
        internal static string lobbyTimerDisplay = "";

        [HarmonyPatch(nameof(GameStartManager.Start))]
        [HarmonyPostfix]
        private static void Start_Postfix(GameStartManager __instance)
        {
            lobbyTimer = 600f;

            __instance.StartButton?.gameObject?.SetUIColors("Icon");
            __instance.EditButton?.gameObject?.SetUIColors("Icon");
            __instance.ClientViewButton?.gameObject?.SetUIColors("Icon");
            __instance.HostViewButton?.gameObject?.SetUIColors("Icon");
        }

        [HarmonyPatch(nameof(GameStartManager.Update))]
        [HarmonyPrefix]
        private static void Update_Prefix(GameStartManager __instance)
        {
            lobbyTimer = Mathf.Max(0f, lobbyTimer -= Time.deltaTime);
            int minutes = (int)lobbyTimer / 60;
            int seconds = (int)lobbyTimer % 60;
            lobbyTimerDisplay = $"{minutes:00}:{seconds:00}";

            __instance.MinPlayers = 1;
        }

        [HarmonyPatch(nameof(GameStartManager.Update))]
        [HarmonyPostfix]
        private static void Update_Postfix(GameStartManager __instance)
        {
            if (!GameState.IsHost)
            {
                __instance.StartButton.gameObject.SetActive(false);
                return;

            }
            __instance.GameStartTextParent.SetActive(false);
            __instance.StartButton.gameObject.SetActive(true);
            if (__instance.startState == GameStartManager.StartingStates.Countdown)
            {
                __instance.StartButton.buttonText.text = string.Format("{0}: {1}", Translator.GetString(StringNames.Cancel), (int)__instance.countDownTimer + 1);
            }
            else
            {
                __instance.StartButton.buttonText.text = Translator.GetString(StringNames.StartLabel);
            }
        }

        [HarmonyPatch(nameof(GameStartManager.BeginGame))]
        [HarmonyPrefix]
        private static bool BeginGame_Prefix(GameStartManager __instance)
        {
            if (__instance.startState == GameStartManager.StartingStates.Countdown)
            {
                SoundManager.instance.StopSound(__instance.gameStartSound);
                __instance.ResetStartState();
                return false;
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                __instance.startState = GameStartManager.StartingStates.Countdown;
                __instance.FinallyBegin();
                return false;
            }

            return true;
        }

        [HarmonyPatch(nameof(GameStartManager.FinallyBegin))]
        [HarmonyPrefix]
        private static void FinallyBegin_Prefix(/*GameStartManager __instance*/)
        {
            Logger.LogHeader($"Game Has Started - {Enum.GetName(typeof(MapNames), GameState.GetActiveMapId)}/{GameState.GetActiveMapId}", "GamePlayManager");
        }
    }
}
