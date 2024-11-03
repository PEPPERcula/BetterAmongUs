using BepInEx;
using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches;

// Code from: https://github.com/tukasa0001/TownOfHost/pull/1265
// Code from: https://github.com/0xDrMoe/TownofHost-Enhanced
[HarmonyPatch(typeof(OptionsMenuBehaviour))]
public static class OptionsMenuBehaviourPatch
{
    private static ClientOptionItem? AntiCheat;
    private static ClientOptionItem? BetterHost;
    private static ClientOptionItem? BetterNotifications;
    private static ClientOptionItem? ForceOwnLanguage;
    private static ClientOptionItem? ChatDarkMode;
    private static ClientOptionItem? ChatInGameplay;
    private static ClientOptionItem? LobbyPlayerInfo;
    private static ClientOptionItem? DisableLobbyTheme;
    private static ClientOptionItem? UnlockFPS;
    private static ClientOptionItem? ShowFPS;
    private static ClientOptionItem? OpenSaveData;
    private static ClientOptionItem? SwitchToVanilla;

    [HarmonyPatch(nameof(OptionsMenuBehaviour.Start))]
    [HarmonyPrefix]
    public static void Start_Postfix(OptionsMenuBehaviour __instance)
    {
        static bool toggleCheckInGamePlay(string buttonName)
        {
            bool flag = GameState.IsInGame && !GameState.IsLobby || GameState.IsFreePlay;
            if (flag)
                BetterNotificationManager.Notify($"Unable to toggle '{buttonName}' while in gameplay!", 2.5f);

            return flag;
        }
        static bool toggleCheckInGame(string buttonName)
        {
            bool flag = GameState.IsInGame;
            if (flag)
                BetterNotificationManager.Notify($"Unable to toggle '{buttonName}' while in game!", 2.5f);

            return flag;
        }

        if (__instance.DisableMouseMovement == null) return;

        if (AntiCheat == null || AntiCheat.ToggleButton == null)
        {
            string title = Translator.GetString("BetterOption.AntiCheat");
            AntiCheat = ClientOptionItem.Create(title, Main.AntiCheat, __instance);
        }

        if (BetterHost == null || BetterHost.ToggleButton == null)
        {
            string title = Translator.GetString("BetterOption.BetterHost");
            BetterHost = ClientOptionItem.Create(title, Main.BetterHost, __instance, BetterHostButtonToggle, () => !toggleCheckInGamePlay(title));
            static void BetterHostButtonToggle()
            {
                RPC.SendBetterCheck();

                foreach (var player in Main.AllPlayerControls)
                {
                    player.BetterData().LastNameSetFor.Clear();
                }

                RPC.SyncAllNames(force: true);
            }
        }

        if (BetterNotifications == null || BetterNotifications.ToggleButton == null)
        {
            string title = Translator.GetString("BetterOption.BetterNotifications");
            BetterNotifications = ClientOptionItem.Create(title, Main.BetterNotifications, __instance, BetterNotificationsToggle);

            static void BetterNotificationsToggle()
            {
                BetterNotificationManager.NotifyQueue.Clear();
                BetterNotificationManager.showTime = 0f;
                BetterNotificationManager.Notifying = false;
            }
        }

        if (ForceOwnLanguage == null || ForceOwnLanguage.ToggleButton == null)
        {
            string title = Translator.GetString("BetterOption.ForceOwnLanguage");
            ForceOwnLanguage = ClientOptionItem.Create(title, Main.ForceOwnLanguage, __instance);
        }

        if (ChatDarkMode == null || ChatDarkMode.ToggleButton == null)
        {
            string title = Translator.GetString("BetterOption.ChatDarkMode");
            ChatDarkMode = ClientOptionItem.Create(title, Main.ChatDarkMode, __instance, ChatDarkModeToggle);

            static void ChatDarkModeToggle()
            {
                ChatPatch.ChatControllerPatch.SetChatTheme();
            }
        }

        if (ChatInGameplay == null || ChatInGameplay.ToggleButton == null)
        {
            string title = Translator.GetString("BetterOption.ChatInGame");
            ChatInGameplay = ClientOptionItem.Create(title, Main.ChatInGameplay, __instance);
        }

        if (LobbyPlayerInfo == null || LobbyPlayerInfo.ToggleButton == null)
        {
            string title = Translator.GetString("BetterOption.LobbyInfo");
            LobbyPlayerInfo = ClientOptionItem.Create(title, Main.LobbyPlayerInfo, __instance);
        }

        if (DisableLobbyTheme == null || DisableLobbyTheme.ToggleButton == null)
        {
            string title = Translator.GetString("BetterOption.LobbyTheme");
            DisableLobbyTheme = ClientOptionItem.Create(title, Main.DisableLobbyTheme, __instance, DisableLobbyThemeButtonToggle);
            static void DisableLobbyThemeButtonToggle()
            {
                if (GameState.IsLobby && !Main.DisableLobbyTheme.Value)
                {
                    SoundManager.instance.CrossFadeSound("MapTheme", LobbyBehaviour.Instance.MapTheme, 0.5f, 1.5f);
                }
            }
        }

        if (UnlockFPS == null || UnlockFPS.ToggleButton == null)
        {
            string title = Translator.GetString("BetterOption.UnlockFPS");
            UnlockFPS = ClientOptionItem.Create(title, Main.UnlockFPS, __instance, UnlockFPSButtonToggle);
            static void UnlockFPSButtonToggle()
            {
                Application.targetFrameRate = Main.UnlockFPS.Value ? 165 : 60;
            }
        }

        if (ShowFPS == null || ShowFPS.ToggleButton == null)
        {
            string title = Translator.GetString("BetterOption.ShowFPS");
            ShowFPS = ClientOptionItem.Create(title, Main.ShowFPS, __instance);
        }

        if (OpenSaveData == null || OpenSaveData.ToggleButton == null)
        {
            string title = Translator.GetString("BetterOption.SaveData");
            OpenSaveData = ClientOptionItem.Create(title, null, __instance, OpenSaveDataButtonToggle, IsToggle: false);
            static void OpenSaveDataButtonToggle()
            {
                if (File.Exists(BetterDataManager.GetFilePath("BetterData")))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = BetterDataManager.GetFilePath("BetterData"),
                        UseShellExecute = true,
                        Verb = "open"
                    });
                }
            }
        }

        if (SwitchToVanilla == null || SwitchToVanilla.ToggleButton == null)
        {
            string title = Translator.GetString("BetterOption.ToVanilla");
            SwitchToVanilla = ClientOptionItem.Create(title, null, __instance, SwitchToVanillaButtonToggle, IsToggle: false, toggleCheck: () => !toggleCheckInGame(title));
            static void SwitchToVanillaButtonToggle()
            {
                ConsoleManager.DetachConsole();
                BetterNotificationManager.BAUNotificationManagerObj.DestroyObj();
                Harmony.UnpatchAll();
            }
        }
    }

    [HarmonyPatch(nameof(OptionsMenuBehaviour.Close))]
    [HarmonyPrefix]
    public static void Close_Postfix()
    {
        ClientOptionItem.CustomBackground?.gameObject.SetActive(false);
    }
}