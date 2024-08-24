using AmongUs.GameOptions;
using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches;


class BetterGameSettings
{
    public static BetterOptionItem? WhenCheating;
    public static BetterOptionItem? InvalidFriendCode;
    public static BetterOptionItem? HideAndSeekImpNum;
}

[HarmonyPatch(typeof(GameSettingMenu))]
static class GameSettingMenuPatch
{
    private static PassiveButton BetterSettingsButton;
    private static GameOptionsMenu BetterSettingsTab;

    public static void SetupSettings(bool IsPreload = false)
    {
        BetterOptionItem.SpacingNum = 0;

        new BetterOptionHeaderItem().Create(BetterSettingsTab, "<color=#4f92ff>Anti-Cheat Settings</color>");

        BetterGameSettings.WhenCheating = new BetterOptionStringItem().Create(0, BetterSettingsTab, "When a player is caught cheating", ["Do Nothing", "Kick", "Ban"], 2);
        BetterGameSettings.InvalidFriendCode = new BetterOptionCheckboxItem().Create(100, BetterSettingsTab, "Allow invalid friendCodes", true);

        if (IsPreload || GameStates.IsHideNSeek)
        {
            if (IsPreload || Main.BetterRoleAlgorithma.Value)
            {
                new BetterOptionHeaderItem().Create(BetterSettingsTab, "<color=#d7d700>Hide & Seek Settings</color>");
                new BetterOptionTitleItem().Create(BetterSettingsTab, $"<color={Utils.GetRoleColor(RoleTypes.Impostor)}>Imposter</color>");
                BetterGameSettings.HideAndSeekImpNum = new BetterOptionIntItem().Create(200, BetterSettingsTab, "# Seekers", [1, 5, 1], 1, "");
            }
        }

        /*
        new BetterOptionCheckboxItem().Create(0, BetterSettingsTab, "CheckBox Test", true);
        new BetterOptionStringItem().Create(1, BetterSettingsTab, "String Test", ["Test 1", "Test 2", "Test 3"]);
        new BetterOptionFloatItem().Create(2, BetterSettingsTab, "Float Test 1", [0f, 180f, 2.5f], 0f, "");
        new BetterOptionIntItem().Create(5, BetterSettingsTab, "Int Test", [0, 5, 1], 0, "");
        new BetterOptionHeaderItem().Create(BetterSettingsTab, "<color=#4f92ff>Test Settings 2</color>");
        */

        if (BetterSettingsTab != null)
            BetterSettingsTab.scrollBar.SetYBoundsMax(0.62f * BetterOptionItem.SpacingNum / 2);
    }

    private static void Initialize()
    {
        _ = new LateTask(() =>
        {
            foreach (var item in BetterOptionItem.BetterOptionItems)
            {
                if (item.TitleText != null)
                {
                    item.TitleText.text = item.Name;
                }
            }
        }, 0.005f, shoudLog: false);
    }

    [HarmonyPatch(nameof(GameSettingMenu.Update))]
    [HarmonyPostfix]
    public static void Update_Postfix(/*GameSettingMenu __instance*/)
    {
        if (BetterSettingsButton != null)
        {
            BetterSettingsButton.buttonText.SetText("Better Settings");

            if (!BetterSettingsButton.selected && !BetterSettingsButton.activeSprites.active)
            {
                BetterSettingsButton.buttonText.color = new Color(0f, 1f, 0f, 1f);
            }
            else
            {
                BetterSettingsButton.buttonText.color = new Color(0.35f, 1f, 0.35f, 1f);
            }
        }
    }

    [HarmonyPatch(nameof(GameSettingMenu.Start))]
    [HarmonyPostfix]
    public static void Start_Postfix(GameSettingMenu __instance)
    {
        __instance.gameObject.transform.SetLocalY(-0.1f);
        GameObject PanelSprite = __instance.gameObject.transform.Find("PanelSprite").gameObject;
        if (PanelSprite != null && !GameStates.IsHideNSeek)
        {
            PanelSprite.transform.SetLocalY(-0.32f);
            PanelSprite.transform.localScale = new Vector3(PanelSprite.transform.localScale.x, 0.625f);
        }

        BetterSettingsButton = UnityEngine.Object.Instantiate(__instance.GameSettingsButton, __instance.GameSettingsButton.transform.parent);

        BetterSettingsButton.name = "BetterSettings";
        BetterSettingsButton.OnClick.RemoveAllListeners();
        BetterSettingsButton.OnMouseOver.RemoveAllListeners();

        if (!GameStates.IsHideNSeek)
        {
            BetterSettingsButton.transform.position = BetterSettingsButton.transform.position - new Vector3(0f, 1.265f, 0f);
        }
        else
        {
            BetterSettingsButton.transform.position = BetterSettingsButton.transform.position - new Vector3(0f, 0.64f, 0f);
        }

        BetterSettingsButton.activeSprites.GetComponent<SpriteRenderer>().color = new Color(0f, 1f, 0.35f, 1f);
        BetterSettingsButton.inactiveSprites.GetComponent<SpriteRenderer>().color = new Color(0f, 1f, 0.35f, 1f);
        BetterSettingsButton.selectedSprites.GetComponent<SpriteRenderer>().color = new Color(0f, 1f, 0.35f, 1f);


        BetterSettingsButton.OnClick.AddListener(new Action(() =>
        {
            __instance.ChangeTab(3, false);
        }));

        BetterSettingsTab = UnityEngine.Object.Instantiate(__instance.GameSettingsTab, __instance.GameSettingsTab.transform.parent);
        BetterSettingsTab.name = "BETTER SETTINGS TAB";
        BetterSettingsTab.scrollBar.Inner.DestroyChildren();

        __instance.GamePresetsButton.OnMouseOver.RemoveAllListeners();
        __instance.GameSettingsButton.OnMouseOver.RemoveAllListeners();
        __instance.RoleSettingsButton.OnMouseOver.RemoveAllListeners();

        __instance.ChangeTab(1, false);

        SetupSettings();
    }

    [HarmonyPatch(nameof(GameSettingMenu.ChangeTab))]
    [HarmonyPrefix]
    public static void ChangeTab_Prefix(GameSettingMenu __instance, [HarmonyArgument(0)] int tabNum, [HarmonyArgument(1)] bool previewOnly)
    {
        if (BetterSettingsTab == null || BetterSettingsButton == null) return;

        BetterSettingsTab.gameObject.SetActive(false);
        BetterSettingsButton.SelectButton(false);

        if ((previewOnly && Controller.currentTouchType == Controller.TouchType.Joystick) || !previewOnly)
        {
            switch (tabNum)
            {
                case 3:
                    BetterSettingsTab.gameObject.SetActive(true);
                    __instance.MenuDescriptionText.text = "Edit better settings for your lobby and gameplay.";
                    break;
            }
        }
    }

    [HarmonyPatch(nameof(GameSettingMenu.ChangeTab))]
    [HarmonyPostfix]
    public static void ChangeTab_Postfix(GameSettingMenu __instance, [HarmonyArgument(0)] int tabNum)
    {
        if (BetterSettingsTab == null || BetterSettingsButton == null) return;

        BetterSettingsButton.buttonText.color = new Color(0f, 1f, 0f, 1f);

        switch (tabNum)
        {
            case 3:
                BetterSettingsButton.SelectButton(true);
                Initialize();
                break;
        }
    }
}