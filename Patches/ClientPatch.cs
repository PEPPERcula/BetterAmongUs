using BetterAmongUs.Helpers;
using BetterAmongUs.Items;
using BetterAmongUs.Managers;
using BetterAmongUs.Modules;
using HarmonyLib;
using Hazel;
using InnerNet;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BetterAmongUs.Patches;

internal class ClientPatch
{
    // Show warning message for newer and older versions of among us
    [HarmonyPatch(typeof(EOSManager))]
    internal class EOSManagerPatch
    {
        [HarmonyPatch(nameof(EOSManager.EndFinalPartsOfLoginFlow))]
        [HarmonyPostfix]
        internal static void EndFinalPartsOfLoginFlow_Postfix()
        {
            UserData.TrySetLocalData();

            var varSupportedVersions = Main.SupportedAmongUsVersions;
            Version currentVersion = new Version(Main.AppVersion);
            Version firstSupportedVersion = new Version(varSupportedVersions.First());
            Version lastSupportedVersion = new Version(varSupportedVersions.Last());

            if (currentVersion > firstSupportedVersion)
            {
                var verText = $"<b>{varSupportedVersions.First()}</b>";
                if (firstSupportedVersion != lastSupportedVersion)
                {
                    verText = $"<b>{varSupportedVersions.Last()}</b> - <b>{varSupportedVersions.First()}</b>";
                }
                Utils.ShowPopUp($"<size=200%>-= <color=#ff2200><b>Warning</b></color> =-</size>\n\n" +
                    $"<size=125%><color=#0dff00>Better Among Us {Main.GetVersionText()}</color>\nsupports <color=#4f92ff>Among Us {verText}</color>,\n" +
                    $"<color=#4f92ff>Among Us <b>{Main.AppVersion}</b></color> is above the supported versions!\n" +
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
                    $"<size=125%><color=#0dff00>Better Among Us {Main.GetVersionText()}</color>\nsupports <color=#4f92ff>Among Us {verText}</color>,\n" +
                    $"<color=#4f92ff>Among Us <b>{Main.AppVersion}</b></color> is below the supported versions!\n" +
                    $"<color=#ae1700>You may encounter minor to game breaking bugs.</color></size>");
            }
        }
    }

    // If developer set account status color to Blue
    [HarmonyPatch(typeof(SignInStatusComponent))]
    internal class SignInStatusComponentPatch
    {
        [HarmonyPatch(nameof(SignInStatusComponent.SetOnline))]
        [HarmonyPrefix]
        internal static bool SetOnline_Prefix(SignInStatusComponent __instance)
        {
            var lines = "<color=#ebbd34>----------------------------------------------------------------------------------------------</color>";
            if (!FileChecker.HasShownWarning && FileChecker.HasUnauthorizedFileOrMod)
            {
                Utils.ShowPopUp($"{lines}\n<b><size=200%><#0DFF00>{Translator.GetString("BetterAmongUs")}</color></size></b>\n<color=#757575><u><size=150%>{FileChecker.WarningMsg}</size></u>\n{lines}");
                FileChecker.HasShownWarning = true;
            }

            if (BannedUserData.CheckLocalBan(out var bannedData))
            {
                __instance.statusSprite.sprite = __instance.guestSprite;
                __instance.glowSprite.sprite = __instance.guestGlow;
                __instance.statusSprite.color = Color.red;
                __instance.glowSprite.color = Color.red;
                __instance.friendsButton.SetActive(false);

                var reason = bannedData.Reason;
                Utils.ShowPopUp($"{lines}\n<b><size=200%><#0DFF00>{Translator.GetString("BetterAmongUs")}</color></size></b>\n<color=#757575><u><size=150%><color=#8f0000>You have been banned\nReason: {reason}</color></size></u>\n{lines}");

                return false;
            }

            if (Main.MyData.IsDev())
            {
                __instance.statusSprite.sprite = __instance.guestSprite;
                __instance.glowSprite.sprite = __instance.guestGlow;
                __instance.statusSprite.color = Color.cyan;
                __instance.glowSprite.color = Color.cyan;
                __instance.friendsButton.SetActive(true);

                return false;
            }

            return true;
        }
    }

    // Log game exit
    [HarmonyPatch(typeof(AmongUsClient))]
    internal class AmongUsClientPatch
    {
        [HarmonyPatch(nameof(AmongUsClient.ExitGame))]
        [HarmonyPostfix]
        internal static void ExitGame_Postfix([HarmonyArgument(0)] DisconnectReasons reason)
        {
            Logger.Log($"Client has left game for: {Enum.GetName(reason)}", "AmongUsClientPatch");
        }

        [HarmonyPatch(nameof(AmongUsClient.OnGameEnd))]
        [HarmonyPrefix]
        internal static void OnGameEnd_Prefix()
        {
            foreach (var data in GameData.Instance.AllPlayers)
            {
                UnityEngine.Object.DontDestroyOnLoad(data.gameObject);
            }

            _ = new LateTask(() =>
            {
                foreach (var data in GameData.Instance.AllPlayers)
                {
                    SceneManager.MoveGameObjectToScene(data.gameObject, SceneManager.GetActiveScene());
                }
            }, 0.6f, shouldLog: false);
        }
    }
    [HarmonyPatch(typeof(InnerNetClient))]
    internal class InnerNetClientPatch
    {
        [HarmonyPatch(nameof(InnerNetClient.SendOrDisconnect))]
        [HarmonyPrefix]
        public static bool SendOrDisconnect_Prefix(InnerNetClient __instance, MessageWriter msg)
        {
            NetworkManager.SendToServer(msg);

            return false;
        }

        [HarmonyPatch(nameof(InnerNetClient.HandleGameData))]
        [HarmonyPrefix]
        internal static bool HandleGameDataInner_Prefix([HarmonyArgument(0)] MessageReader oldReader)
        {
            NetworkManager.HandleGameData(oldReader);
            return false;
        }

        [HarmonyPatch(nameof(InnerNetClient.CanBan))]
        [HarmonyPrefix]
        internal static bool CanBan_Prefix(ref bool __result)
        {
            __result = GameState.IsHost;
            return false;
        }

        [HarmonyPatch(nameof(InnerNetClient.CanKick))]
        [HarmonyPrefix]
        internal static bool CanKick_Prefix(ref bool __result)
        {
            __result = GameState.IsHost || (GameState.IsInGamePlay && (GameState.IsMeeting || GameState.IsExilling));
            return false;
        }

        [HarmonyPatch(nameof(InnerNetClient.KickPlayer))]
        [HarmonyPrefix]
        internal static void KickPlayer_Prefix(ref int clientId, ref bool ban)
        {
            if (ban && BetterGameSettings.UseBanPlayerList.GetBool())
            {
                NetworkedPlayerInfo info = Utils.PlayerFromClientId(clientId).Data;
                BetterDataManager.SaveBanList(info.FriendCode, info.Puid);
            }
        }
    }
    // Set text color
    [HarmonyPatch(typeof(CosmeticsLayer))]
    internal class CosmeticsLayerPatch
    {
        [HarmonyPatch(nameof(CosmeticsLayer.GetColorBlindText))]
        [HarmonyPrefix]
        internal static bool GetColorBlindText_Prefix(CosmeticsLayer __instance, ref string __result)
        {
            if (__instance.bodyMatProperties.ColorId > Palette.PlayerColors.Length) return true;

            string colorName = Palette.GetColorName(__instance.bodyMatProperties.ColorId);

            if (!string.IsNullOrEmpty(colorName))
            {
                __result = (char.ToUpperInvariant(colorName[0]) + colorName.Substring(1).ToLowerInvariant()).ToColor(Palette.PlayerColors[__instance.bodyMatProperties.ColorId]);
            }
            else
            {
                __result = string.Empty;
            }

            return false;
        }
    }
    // Clean up menu
    [HarmonyPatch(typeof(RegionMenu))]
    internal class RegionMenuPatch
    {
        [HarmonyPatch(nameof(RegionMenu.OnEnable))]
        [HarmonyPostfix]
        internal static void AdjustButtonPositions_Postfix(RegionMenu __instance)
        {
            const int maxColumns = 4;
            int buttonsPerColumn = 6;
            float buttonSpacing = 0.6f;
            float buttonSpacingSide = 2.25f;

            List<UiElement> buttons = __instance.controllerSelectable.ToArray().ToList();

            int columnCount = (buttons.Count + buttonsPerColumn - 1) / buttonsPerColumn;

            while (columnCount > maxColumns)
            {
                buttonsPerColumn++;
                columnCount = (buttons.Count + buttonsPerColumn - 1) / buttonsPerColumn;
            }

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
    // fix report name on anti cheat ban
    [HarmonyPatch(typeof(ReportReasonScreen))]
    class ReportReasonScreenPatch
    {
        [HarmonyPatch(nameof(ReportReasonScreen.Show))]
        [HarmonyPrefix]
        internal static void Show_Prefix(ref string playerName)
        {
            if (Utils.IsHtmlText(playerName))
            {
                string extractedText = playerName.Split(new[] { "<color=#ffea00>", "</color>" }, StringSplitOptions.None)[1];
                playerName = extractedText;
            }
        }
    }
}
