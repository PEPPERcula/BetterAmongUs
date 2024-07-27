using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches;

internal class MainMenuPatch
{
    private static readonly string OnlineMsg = "Online access has been temporarily disabled!";

    // Handle FileChecker
    [HarmonyPatch(typeof(AccountManager))]
    internal class AccountManagerPatch
    {
        [HarmonyPatch(nameof(AccountManager.CanPlayOnline))]
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (!FileChecker.HasShownPopUp && FileChecker.CheckIfUnauthorizedFiles())
            {
                var lines = "<color=#ebbd34>----------------------------------------------------------------------------------------------</color>";
                var icon = "<color=#278720>♻</color>";
                var warning = "<color=#e60000>⚠</color>";
                FileChecker.HasShownPopUp = true;
                DisconnectPopup.Instance._textArea.enableWordWrapping = false;
                DisconnectPopup.Instance.ShowCustom($"{lines}\n<b><size=200%>{icon}<color=#0ed400>Better Among Us</color>{icon}</size></b>\n<color=#757575><u><size=150%>{warning}<color=#8f0000>{FileChecker.UnauthorizedReason}</color>{warning}</size></u>\n\n<color=white>\n{OnlineMsg}\n{lines}");
            }
        }
    }

    // Replace AU logo with BAU logo
    [HarmonyPatch(typeof(MainMenuManager))]
    internal class MainMenuManagerPatch
    {
        [HarmonyPatch(nameof(MainMenuManager.Start))]
        [HarmonyPostfix]
        public static void Postfix(/*MainMenuManager __instance*/)
        {
            GameObject logo = GameObject.Find("LeftPanel/Sizer/LOGO-AU");
            GameObject sizer = logo.transform.parent.gameObject;
            sizer.transform.localPosition = new Vector3(sizer.transform.localPosition.x, sizer.transform.localPosition.y - 0.035f, sizer.transform.localPosition.z);
            sizer.transform.position = new Vector3(sizer.transform.position.x, sizer.transform.position.y, -0.5f);
            logo.transform.localScale = new Vector3(0.003f, 0.0025f, 0f);
            logo.GetComponent<SpriteRenderer>().sprite = Utils.LoadSprite("BetterAmongUs.Resources.Images.BetterAmongUs-Logo.png", 1f);
        }
    }
}
