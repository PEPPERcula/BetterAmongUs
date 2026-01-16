using BepInEx;
using BetterAmongUs.Data;
using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
using BetterAmongUs.Modules;
using BetterAmongUs.Mono;
using BetterAmongUs.Patches.Gameplay.UI.Chat;
using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches.Client;

[HarmonyPatch(typeof(OptionsMenuBehaviour))]
internal static class OptionsMenuBehaviourPatch
{
    private static ClientOptionItem? AntiCheat;
    private static ClientOptionItem? SendBetterRpc;
    private static ClientOptionItem? BetterNotifications;
    private static ClientOptionItem? UnlockFPS;
    private static ClientOptionItem? ShowFPS;
    private static ClientOptionItem? ForceOwnLanguage;
    private static ClientOptionItem? ChatDarkMode;
    private static ClientOptionItem? ChatInGameplay;
    private static ClientOptionItem? LobbyPlayerInfo;
    private static ClientOptionItem? LobbyTheme;
    private static ClientOptionItem? ButtonCooldownInDecimalUnder10s;
    private static ClientOptionItem? TryFixStuttering;
    private static ClientOptionItem? OpenSaveData;
    private static ClientOptionItem? SwitchToVanilla;

    [HarmonyPatch(nameof(OptionsMenuBehaviour.Start))]
    [HarmonyPrefix]
    private static void Start_Postfix(OptionsMenuBehaviour __instance)
    {
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
            AntiCheat = ClientOptionItem.Create(title, BAUPlugin.AntiCheat, __instance);
        }

        if (SendBetterRpc == null || SendBetterRpc.ToggleButton == null)
        {
            string title = Translator.GetString("BetterOption.SendBetterRpc");
            SendBetterRpc = ClientOptionItem.Create(title, BAUPlugin.SendBetterRpc, __instance, SendBetterRpcToggle);

            static void SendBetterRpcToggle()
            {
                if (GameState.IsInGame)
                {
                    foreach (var player in BAUPlugin.AllPlayerControls)
                    {
                        if (player.IsLocalPlayer()) continue;
                        player.BetterData().HandshakeHandler.ResendSecretToPlayer();
                    }
                }
            }
        }

        if (BetterNotifications == null || BetterNotifications.ToggleButton == null)
        {
            string title = Translator.GetString("BetterOption.BetterNotifications");
            BetterNotifications = ClientOptionItem.Create(title, BAUPlugin.BetterNotifications, __instance, BetterNotificationsToggle);

            static void BetterNotificationsToggle()
            {
                BetterNotificationManager.NotifyQueue.Clear();
                BetterNotificationManager.showTime = 0f;
                BetterNotificationManager.Notifying = false;
            }
        }

        if (UnlockFPS == null || UnlockFPS.ToggleButton == null)
        {
            string title = Translator.GetString("BetterOption.UnlockFPS");
            UnlockFPS = ClientOptionItem.Create(title, BAUPlugin.UnlockFPS, __instance, UnlockFPSButtonToggle);
            static void UnlockFPSButtonToggle()
            {
                Application.targetFrameRate = BAUPlugin.UnlockFPS.Value ? 165 : 60;
            }
        }

        if (ShowFPS == null || ShowFPS.ToggleButton == null)
        {
            string title = Translator.GetString("BetterOption.ShowFPS");
            ShowFPS = ClientOptionItem.Create(title, BAUPlugin.ShowFPS, __instance);
        }

        if (ForceOwnLanguage == null || ForceOwnLanguage.ToggleButton == null)
        {
            string title = Translator.GetString("BetterOption.ForceOwnLanguage");
            ForceOwnLanguage = ClientOptionItem.Create(title, BAUPlugin.ForceOwnLanguage, __instance);
        }

        if (ChatDarkMode == null || ChatDarkMode.ToggleButton == null)
        {
            string title = Translator.GetString("BetterOption.ChatDarkMode");
            ChatDarkMode = ClientOptionItem.Create(title, BAUPlugin.ChatDarkMode, __instance, ChatDarkModeToggle);

            static void ChatDarkModeToggle()
            {
                ChatPatch.ChatControllerPatch.SetChatTheme();
            }
        }

        if (ChatInGameplay == null || ChatInGameplay.ToggleButton == null)
        {
            string title = Translator.GetString("BetterOption.ChatInGame");
            ChatInGameplay = ClientOptionItem.Create(title, BAUPlugin.ChatInGameplay, __instance);
        }

        if (LobbyPlayerInfo == null || LobbyPlayerInfo.ToggleButton == null)
        {
            string title = Translator.GetString("BetterOption.LobbyInfo");
            LobbyPlayerInfo = ClientOptionItem.Create(title, BAUPlugin.LobbyPlayerInfo, __instance);
        }

        if (LobbyTheme == null || LobbyTheme.ToggleButton == null)
        {
            string title = Translator.GetString("BetterOption.LobbyTheme");
            LobbyTheme = ClientOptionItem.Create(title, BAUPlugin.LobbyTheme, __instance, LobbyThemeButtonToggle);
            static void LobbyThemeButtonToggle()
            {
                if (GameState.IsLobby && BAUPlugin.LobbyTheme.Value)
                {
                    SoundManager.instance.CrossFadeSound("MapTheme", LobbyBehaviour.Instance.MapTheme, 0.5f, 1.5f);
                }
            }
        }

        if (ButtonCooldownInDecimalUnder10s == null || ButtonCooldownInDecimalUnder10s.ToggleButton == null)
        {
            string title = Translator.GetString("BetterOption.ButtonCooldownInDecimalUnder10s");
            ButtonCooldownInDecimalUnder10s = ClientOptionItem.Create(title, BAUPlugin.ButtonCooldownInDecimalUnder10s, __instance);
        }

        if (TryFixStuttering == null || TryFixStuttering.ToggleButton == null)
        {
            string title = Translator.GetString("BetterOption.TryFixStuttering");
            TryFixStuttering = ClientOptionItem.Create(title, BAUPlugin.TryFixStuttering, __instance, TryFixStutteringButtonToggle);
            [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
            static void TryFixStutteringButtonToggle()
            {
                if (BAUPlugin.TryFixStuttering.Value)
                {
                    if (Application.platform == RuntimePlatform.WindowsPlayer && Environment.ProcessorCount >= 4)
                    {
                        var process = Process.GetCurrentProcess();
                        BAUPlugin.OriginalAffinity = process.ProcessorAffinity;
                        process.ProcessorAffinity = (IntPtr)((1 << 2) | (1 << 3));
                    }
                }
                else
                {
                    if (BAUPlugin.OriginalAffinity.HasValue)
                    {
                        var proc = Process.GetCurrentProcess();
                        proc.ProcessorAffinity = BAUPlugin.OriginalAffinity.Value;
                        BAUPlugin.OriginalAffinity = null;
                    }
                }
            }
        }

        if (OpenSaveData == null || OpenSaveData.ToggleButton == null)
        {
            string title = Translator.GetString("BetterOption.SaveData");
            OpenSaveData = ClientOptionItem.Create(title, null, __instance, OpenSaveDataButtonToggle, IsToggle: false);
            static void OpenSaveDataButtonToggle()
            {
                if (File.Exists(BetterDataManager.dataPath))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = BetterDataManager.dataPath,
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
                ModManager.Instance.ModStamp.gameObject.SetActive(false);
                SceneChanger.ChangeScene("MainMenu");
            }
        }
    }

    [HarmonyPatch(nameof(OptionsMenuBehaviour.Close))]
    [HarmonyPrefix]
    private static void Close_Postfix()
    {
        ClientOptionItem.CustomBackground?.gameObject.SetActive(false);
    }
}
