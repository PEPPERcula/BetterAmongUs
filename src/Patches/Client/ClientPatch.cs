using BepInEx.Unity.IL2CPP.Utils;
using BetterAmongUs.Data;
using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
using BetterAmongUs.Modules;
using BetterAmongUs.Patches.Gameplay.UI.Chat;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using HarmonyLib;
using Hazel;
using InnerNet;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BetterAmongUs.Patches.Client;

internal static class ClientPatch
{
    [HarmonyPatch(typeof(AccountTab))]
    internal static class AccountTabPatch
    {
        [HarmonyPatch(nameof(AccountTab.Awake))]
        [HarmonyPostfix]
        private static void Awake_Postfix(AccountTab __instance)
        {
            __instance.signInStatusComponent.friendsButton.SetUIColors();
        }
    }

    // If developer set account status color to Blue
    [HarmonyPatch(typeof(SignInStatusComponent))]
    internal static class SignInStatusComponentPatch
    {
        [HarmonyPatch(nameof(SignInStatusComponent.SetOnline))]
        [HarmonyPrefix]
        private static bool SetOnline_Prefix(SignInStatusComponent __instance)
        {
            var varSupportedVersions = BAUPlugin.SupportedAmongUsVersions;
            Version currentVersion = new(BAUPlugin.AppVersion);
            Version firstSupportedVersion = new(varSupportedVersions.First());
            Version lastSupportedVersion = new(varSupportedVersions.Last());

            if (currentVersion > firstSupportedVersion)
            {
                var verText = $"<b>{varSupportedVersions.First()}</b>";
                if (firstSupportedVersion != lastSupportedVersion)
                {
                    verText = $"<b>{varSupportedVersions.Last()}</b> - <b>{varSupportedVersions.First()}</b>";
                }

                Utils.ShowPopUp($"<size=200%>-= <color=#ff2200><b>Warning</b></color> =-</size>\n\n" +
                    $"<size=125%><color=#0dff00>Better Among Us {BAUPlugin.GetVersionText()}</color>\nsupports <color=#4f92ff>Among Us {verText}</color>,\n" +
                    $"<color=#4f92ff>Among Us <b>{BAUPlugin.AppVersion}</b></color> is above the supported versions!\n" +
                    $"<color=#ae1700>You may encounter minor to game breaking bugs.</color></size>");
            }
            else if (currentVersion < lastSupportedVersion)
            {
                var verText = $"<b>{varSupportedVersions.First()}</b>";
                if (firstSupportedVersion != lastSupportedVersion)
                {
                    verText = $"<b>{varSupportedVersions.Last()}</b> - <b>{varSupportedVersions.First()}</b>";
                }

                Utils.ShowPopUp($"<size=200%>-= <color=#ff2200><b>Warning</b></color> =-</size>\n\n" +
                    $"<size=125%><color=#0dff00>Better Among Us {BAUPlugin.GetVersionText()}</color>\nsupports <color=#4f92ff>Among Us {verText}</color>,\n" +
                    $"<color=#4f92ff>Among Us <b>{BAUPlugin.AppVersion}</b></color> is below the supported versions!\n" +
                    $"<color=#ae1700>You may encounter minor to game breaking bugs.</color></size>");
            }

            return true;
        }
    }

    // Log game exit
    [HarmonyPatch(typeof(AmongUsClient))]
    internal static class AmongUsClientPatch
    {
        [HarmonyPatch(nameof(AmongUsClient.ExitGame))]
        [HarmonyPostfix]
        private static void ExitGame_Postfix([HarmonyArgument(0)] DisconnectReasons reason)
        {
            CustomLoadingBarManager.ToggleLoadingBar(false);
            Logger_.Log($"Client has left game for: {Enum.GetName(reason)}", "AmongUsClientPatch");
        }

        [HarmonyPatch(nameof(AmongUsClient.OnGameEnd))]
        [HarmonyPrefix]
        private static void OnGameEnd_Prefix()
        {
            foreach (var data in GameData.Instance.AllPlayers)
            {
                UnityEngine.Object.DontDestroyOnLoad(data.gameObject);
            }

            LateTask.Schedule(() =>
            {
                foreach (var data in GameData.Instance.AllPlayers)
                {
                    SceneManager.MoveGameObjectToScene(data.gameObject, SceneManager.GetActiveScene());
                }
            }, 0.6f, shouldLog: false);
        }

        [HarmonyPatch(nameof(AmongUsClient.CoStartGame))]
        [HarmonyPostfix]
        private static void CoStartGame_Postfix(AmongUsClient __instance)
        {
            if (BAUPlugin.ChatInGameplay.Value)
            {
                ChatPatch.ClearChat();
            }
            __instance.StartCoroutine(CoLoading());
        }

        private static IEnumerator CoLoading()
        {
            CustomLoadingBarManager.ToggleLoadingBar(true);

            if (GameState.IsHost)
            {
                yield return CoLoadingHost();
            }
            else
            {
                yield return CoLoadingClient();
            }

            CustomLoadingBarManager.SetLoadingPercent(100f, "Complete");
            yield return new WaitForSeconds(0.25f);
            CustomLoadingBarManager.ToggleLoadingBar(false);
        }

        private static IEnumerator CoLoadingHost()
        {
            var client = AmongUsClient.Instance.GetClient(AmongUsClient.Instance.ClientId);
            var clients = AmongUsClient.Instance.allClients;

            while (BAUPlugin.AllPlayerControls.Count > 0 && BAUPlugin.AllPlayerControls.Any(pc => !pc.roleAssigned))
            {
                if (!GameState.IsInGame)
                {
                    CustomLoadingBarManager.ToggleLoadingBar(false);
                    yield break;
                }

                string loadingText = "Initializing Game";
                float progress = 0f;

                if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
                {
                    loadingText = "Starting Game Session";
                    progress = 0.1f;
                }
                else if (LobbyBehaviour.Instance)
                {
                    loadingText = "Loading";
                    progress = 0.2f;
                }
                else if (!ShipStatus.Instance || AmongUsClient.Instance.ShipLoadingAsyncHandle.IsValid())
                {
                    bool isShipLoading = AmongUsClient.Instance.ShipLoadingAsyncHandle.IsValid();

                    loadingText = isShipLoading ? "Loading Ship Async" : "Spawning Ship";
                    progress = isShipLoading ? 0.3f : 0.4f;
                }
                else if (BAUPlugin.AllPlayerControls.Any(player => !player.roleAssigned))
                {
                    int totalPlayers = BAUPlugin.AllPlayerControls.Count;
                    int assignedPlayers = BAUPlugin.AllPlayerControls.Count(pc => pc.roleAssigned);
                    float assignmentProgress = (float)assignedPlayers / Mathf.Max(1, totalPlayers);

                    loadingText = $"Assigning Roles ({assignedPlayers}/{totalPlayers})";
                    progress = 0.4f + 0.3f * assignmentProgress;
                }
                else if (!client.IsReady)
                {
                    int readyClients = clients.CountIl2Cpp(c => c?.Character != null && c.IsReady);
                    int totalClients = clients.CountIl2Cpp(c => c?.Character != null);

                    loadingText = $"Waiting for Players ({readyClients}/{totalClients})";
                    progress = 0.8f + 0.2f * readyClients / Mathf.Max(1, totalClients);
                }

                int percent = Mathf.RoundToInt(progress * 100f);
                CustomLoadingBarManager.SetLoadingPercent(percent, loadingText);

                yield return null;
            }
        }

        private static IEnumerator CoLoadingClient()
        {
            var client = AmongUsClient.Instance.GetClient(AmongUsClient.Instance.ClientId);
            var clients = AmongUsClient.Instance.allClients;

            while (BAUPlugin.AllPlayerControls.Count > 0 && BAUPlugin.AllPlayerControls.Any(pc => !pc.roleAssigned))
            {

                if (GameState.IsHost)
                {
                    yield return CoLoadingHost();
                    yield break;
                }

                if (!GameState.IsInGame)
                {
                    CustomLoadingBarManager.ToggleLoadingBar(false);
                    yield break;
                }

                string loadingText = "Initializing Game";
                float progress = 0;

                if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
                {
                    loadingText = "Starting Game Session";
                    progress = 0.1f;
                }
                else if (LobbyBehaviour.Instance)
                {
                    loadingText = "Loading";
                    progress = 0.25f;
                }
                else if (!ShipStatus.Instance || AmongUsClient.Instance.ShipLoadingAsyncHandle.IsValid())
                {
                    bool isShipLoading = AmongUsClient.Instance.ShipLoadingAsyncHandle.IsValid();

                    loadingText = isShipLoading ? "Loading Ship Async" : "Spawning Ship";
                    progress = isShipLoading ? 0.35f : 0.4f;
                }
                else if (!client.IsReady)
                {
                    loadingText = "Finalizing Connection";
                    progress = 0.75f;
                }
                else
                {
                    int readyClients = clients.CountIl2Cpp(c => c?.Character != null && c.IsReady);
                    int totalClients = clients.CountIl2Cpp(c => c?.Character != null);

                    loadingText = $"Waiting for Players ({readyClients}/{totalClients})";
                    progress = 0.85f + 0.15f * readyClients / Mathf.Max(1, totalClients);
                }

                int percent = Mathf.RoundToInt(progress * 100f);
                CustomLoadingBarManager.SetLoadingPercent(percent, loadingText);

                yield return null;
            }
        }
    }

    [HarmonyPatch(typeof(InnerNetClient))]
    internal static class InnerNetClientPatch
    {
        [HarmonyPatch(nameof(InnerNetClient.SendOrDisconnect))]
        [HarmonyPrefix]
        private static bool SendOrDisconnect_Prefix(InnerNetClient __instance, MessageWriter msg)
        {
            NetworkManager.SendToServer(msg);

            return false;
        }

        [HarmonyPatch(nameof(InnerNetClient.HandleGameData))]
        [HarmonyPrefix]
        private static bool HandleGameDataInner_Prefix([HarmonyArgument(0)] MessageReader oldReader)
        {
            NetworkManager.HandleGameData(oldReader);
            return false;
        }

        [HarmonyPatch(nameof(InnerNetClient.CanBan))]
        [HarmonyPrefix]
        private static bool CanBan_Prefix(ref bool __result)
        {
            __result = GameState.IsHost;
            return false;
        }

        [HarmonyPatch(nameof(InnerNetClient.CanKick))]
        [HarmonyPrefix]
        private static bool CanKick_Prefix(ref bool __result)
        {
            __result = GameState.IsHost || GameState.IsInGamePlay && (GameState.IsMeeting || GameState.IsExilling);
            return false;
        }

        [HarmonyPatch(nameof(InnerNetClient.KickPlayer))]
        [HarmonyPrefix]
        private static void KickPlayer_Prefix(ref int clientId, ref bool ban)
        {
            if (ban && BetterGameSettings.UseBanPlayerList.GetBool())
            {
                NetworkedPlayerInfo info = Utils.PlayerFromClientId(clientId).Data;
                BetterDataManager.AddToBanList(info.FriendCode, info.Puid);
            }
        }
    }
}
