using HarmonyLib;
using Hazel;
using UnityEngine;

namespace BetterAmongUs.Patches;

// Code from: https://github.com/tukasa0001/TownOfHost/pull/1265
// Code from: https://github.com/0xDrMoe/TownofHost-Enhanced
[HarmonyPatch(typeof(OptionsMenuBehaviour))]
public static class OptionsMenuBehaviourPatch
{
    private static ClientOptionItem AntiCheat;
    private static ClientOptionItem BetterHost;
    private static ClientOptionItem BetterRoleAlgorithma;
    private static ClientOptionItem LobbyPlayerInfo;
    private static ClientOptionItem DisableLobbyTheme;
    private static ClientOptionItem UnlockFPS;
    private static ClientOptionItem ShowFPS;
    private static ClientOptionItem OpenSaveData;

    [HarmonyPatch(nameof(OptionsMenuBehaviour.Start))]
    [HarmonyPrefix]
    public static void Start_Postfix(OptionsMenuBehaviour __instance)
    {
        static bool toggleCheckInGamePlay(string buttonName)
        {
            bool flag = GameStates.IsInGame && !GameStates.IsLobby || GameStates.IsFreePlay;
            if (flag)
                BetterNotificationManager.Notify($"Unable to toggle {buttonName} while in gameplay!", 2.5f);

            return flag;
        }
        static bool toggleCheckInGame(string buttonName)
        {
            bool flag = GameStates.IsInGame;
            if (flag)
                BetterNotificationManager.Notify($"Unable to toggle {buttonName} while in game!", 2.5f);

            return flag;
        }

        if (__instance.DisableMouseMovement == null) return;

        if (AntiCheat == null || AntiCheat.ToggleButton == null)
        {
            AntiCheat = ClientOptionItem.Create("<color=#4f92ff>Anti-Cheat</color>", Main.AntiCheat, __instance);
        }

        if (BetterHost == null || BetterHost.ToggleButton == null)
        {
            BetterHost = ClientOptionItem.Create("<color=#4f92ff>Better Host</color>", Main.BetterHost, __instance, BetterHostButtonToggle, () => !toggleCheckInGamePlay("<color=#4f92ff>Better Host</color>"));
            static void BetterHostButtonToggle()
            {
                var flag = GameStates.IsHost && Main.BetterHost.Value;
                MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, unchecked((byte)CustomRPC.BetterCheck), SendOption.None, -1);
                messageWriter.Write((byte)PlayerControl.LocalPlayer.NetId);
                messageWriter.Write(flag);
                AmongUsClient.Instance.FinishRpcImmediately(messageWriter);

                RPC.SyncAllNames(force: true);
            }
        }

        if (BetterRoleAlgorithma == null || BetterRoleAlgorithma.ToggleButton == null)
        {
            BetterRoleAlgorithma = ClientOptionItem.Create("<color=#4f92ff>Better Role Algorithma</color>", Main.BetterRoleAlgorithma, __instance, toggleCheck: () => !toggleCheckInGamePlay("<color=#4f92ff>Better Role Algorithma</color>"));
        }

        if (LobbyPlayerInfo == null || LobbyPlayerInfo.ToggleButton == null)
        {
            LobbyPlayerInfo = ClientOptionItem.Create("Show Lobby Info", Main.LobbyPlayerInfo, __instance);
        }

        if (DisableLobbyTheme == null || DisableLobbyTheme.ToggleButton == null)
        {
            DisableLobbyTheme = ClientOptionItem.Create("Disable Lobby Theme", Main.DisableLobbyTheme, __instance, DisableLobbyThemeButtonToggle);
            static void DisableLobbyThemeButtonToggle()
            {
                if (GameStates.IsLobby && !Main.DisableLobbyTheme.Value)
                {
                    SoundManager.instance.CrossFadeSound("MapTheme", LobbyBehaviour.Instance.MapTheme, 0.5f, 1.5f);
                }
            }
        }

        if (UnlockFPS == null || UnlockFPS.ToggleButton == null)
        {
            UnlockFPS = ClientOptionItem.Create("UnlockFPS", Main.UnlockFPS, __instance, UnlockFPSButtonToggle);
            static void UnlockFPSButtonToggle()
            {
                Application.targetFrameRate = Main.UnlockFPS.Value ? 165 : 60;
            }
        }

        if (ShowFPS == null || ShowFPS.ToggleButton == null)
        {
            ShowFPS = ClientOptionItem.Create("ShowFPS", Main.ShowFPS, __instance);
        }

        if (OpenSaveData == null || OpenSaveData.ToggleButton == null)
        {
            DisableLobbyTheme = ClientOptionItem.Create("Open Save Data", null, __instance, OpenSaveDataButtonToggle, IsToggle: false);
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
    }

    [HarmonyPatch(nameof(OptionsMenuBehaviour.Close))]
    [HarmonyPrefix]
    public static void Close_Postfix()
    {
        ClientOptionItem.CustomBackground?.gameObject.SetActive(false);
    }
}