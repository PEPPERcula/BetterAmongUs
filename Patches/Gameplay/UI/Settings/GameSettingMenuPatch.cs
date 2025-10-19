using AmongUs.GameOptions;
using BetterAmongUs.Helpers;
using BetterAmongUs.Items.OptionItems;
using BetterAmongUs.Items.OptionItems.NoneOption;
using BetterAmongUs.Modules;
using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches.Gameplay.UI.Settings;

class BetterGameSettings
{
    internal static OptionItem? WhenCheating;
    internal static OptionItem? InvalidFriendCode;
    internal static OptionItem? UseBanPlayerList;
    internal static OptionItem? UseBanNameList;
    internal static OptionItem? UseBanWordList;
    internal static OptionItem? UseBanWordListOnlyLobby;
    internal static OptionItem? HideAndSeekImpNum;
    internal static OptionItem? DetectedLevelAbove;
    internal static OptionItem? DetectCheatClients;
    internal static OptionItem? DetectInvalidRPCs;
    internal static OptionItem? RoleRandomizer;
    internal static OptionItem? DesyncRoles;
    internal static OptionItem? CancelInvalidSabotage;
    internal static OptionItem? CensorDetectionReason;
    internal static OptionItem? RemovePetOnDeath;
    internal static OptionItem? DisableSabotages;
}

class BetterGameSettingsTemp
{
    internal static OptionItem? HideAndSeekImp2;
    internal static OptionItem? HideAndSeekImp3;
    internal static OptionItem? HideAndSeekImp4;
    internal static OptionItem? HideAndSeekImp5;
}

[HarmonyPatch(typeof(GameSettingMenu))]
static class GameSettingMenuPatch
{
    internal static OptionTab? BetterSettingsTab;

    internal static void SetupSettings(bool IsPreload = false)
    {
        // Use 1700 next ID

        BetterSettingsTab = OptionTab.Create(3, "BetterSetting", "BetterSetting.Description", Color.green);

        // Anti-Cheat Settings
        {
            OptionHeaderItem.Create(BetterSettingsTab, "BetterSetting.MainHeader.AntiCheat");

            if (IsPreload || GameState.IsHost)
            {
                OptionTitleItem.Create(BetterSettingsTab, "BetterSetting.TextHeader.HostOnly");
                BetterGameSettings.WhenCheating = OptionStringItem.Create(100, BetterSettingsTab, "BetterSetting.Setting.WhenCheating",
                    ["BetterSetting.Setting.WhenCheating.Notify", "BetterSetting.Setting.WhenCheating.Kick", "BetterSetting.Setting.WhenCheating.Ban"], 2);
                BetterGameSettings.InvalidFriendCode = OptionCheckboxItem.Create(200, BetterSettingsTab, "BetterSetting.Setting.InvalidFriendCode", true);
                BetterGameSettings.CancelInvalidSabotage = OptionCheckboxItem.Create(900, BetterSettingsTab, "BetterSetting.Setting.CancelInvalidSabotage", true);
                BetterGameSettings.UseBanPlayerList = OptionCheckboxItem.Create(300, BetterSettingsTab, "BetterSetting.Setting.UseBanPlayerList", true);
                BetterGameSettings.UseBanNameList = OptionCheckboxItem.Create(400, BetterSettingsTab, "BetterSetting.Setting.UseBanNameList", true);
                BetterGameSettings.UseBanWordList = OptionCheckboxItem.Create(500, BetterSettingsTab, "BetterSetting.Setting.UseBanWordList", true);
                BetterGameSettings.UseBanWordListOnlyLobby = OptionCheckboxItem.Create(1400, BetterSettingsTab, "BetterSetting.Setting.UseBanWordListOnlyLobby", true, BetterGameSettings.UseBanWordList);
                OptionDividerItem.Create(BetterSettingsTab);
            }

            OptionTitleItem.Create(BetterSettingsTab, "BetterSetting.TextHeader.Detections");
            BetterGameSettings.CensorDetectionReason = OptionCheckboxItem.Create(1300, BetterSettingsTab, "BetterSetting.Setting.CensorDetectionReason", false);
            BetterGameSettings.DetectedLevelAbove = OptionIntItem.Create(600, BetterSettingsTab, "BetterSetting.Setting.DetectedLevelAbove", (100, 5000, 5), 200, ("Lv ", ""));
            BetterGameSettings.DetectCheatClients = OptionCheckboxItem.Create(700, BetterSettingsTab, "BetterSetting.Setting.DetectCheatClients", true);
            BetterGameSettings.DetectInvalidRPCs = OptionCheckboxItem.Create(800, BetterSettingsTab, "BetterSetting.Setting.DetectInvalidRPCs", true);

            /*
            TitleList.Add(new BetterOptionDividerItem().Create(BetterSettingsTab));
            TitleList.Add(new BetterOptionTitleItem().Create(BetterSettingsTab, $"<color=#f20>Experimental</color>"));
            BetterGameSettings.CancelInvalidSabotage = new BetterOptionCheckboxItem().Create(100000, BetterSettingsTab, "Detect Invalid Sabotages", false);
            */
        }

        if (IsPreload || GameState.IsHost)
        {
            OptionHeaderItem.Create(BetterSettingsTab, "BetterSetting.MainHeader.RoleAlgorithm");
            BetterGameSettings.RoleRandomizer = OptionStringItem.Create(1100, BetterSettingsTab, "BetterSetting.Setting.RoleRandomizer", ["System.Random", "UnityEngine.Random"], 0);
            BetterGameSettings.DesyncRoles = OptionCheckboxItem.Create(1200, BetterSettingsTab, "BetterSetting.Setting.DesyncRoles", true);
        }

        // Gameplay Settings
        {
            if (IsPreload || GameState.IsHost && GameState.IsPrivateOnlyLobby)
            {
                if (IsPreload || !GameState.IsHideNSeek)
                {
                    OptionHeaderItem.Create(BetterSettingsTab, "BetterSetting.MainHeader.Gameplay");
                    BetterGameSettings.DisableSabotages = OptionCheckboxItem.Create(1500, BetterSettingsTab, "BetterSetting.Setting.DisableSabotages", false);
                    BetterGameSettings.RemovePetOnDeath = OptionCheckboxItem.Create(1600, BetterSettingsTab, "BetterSetting.Setting.RemovePetOnDeath", false);
                }
                else if (IsPreload || GameState.IsHideNSeek)
                {
                    OptionHeaderItem.Create(BetterSettingsTab, "BetterSetting.MainHeader.HideNSeek");
                    OptionTitleItem.Create(BetterSettingsTab, $"<color={RoleTypes.Impostor.GetRoleHex()}>{Translator.GetString(StringNames.ImpostorsCategory)}</color>");
                    BetterGameSettings.HideAndSeekImpNum = OptionIntItem.Create(1000, BetterSettingsTab, "BetterSetting.Setting.HideAndSeekImpNum", (1, 5, 1), 1);

                    BetterGameSettingsTemp.HideAndSeekImp2 = OptionPlayerItem.Create(0, BetterSettingsTab, "BetterSetting.TempSetting.HideAndSeekImpNum", BetterGameSettings.HideAndSeekImpNum);
                    BetterGameSettingsTemp.HideAndSeekImp3 = OptionPlayerItem.Create(1, BetterSettingsTab, "BetterSetting.TempSetting.HideAndSeekImpNum", BetterGameSettings.HideAndSeekImpNum);
                    BetterGameSettingsTemp.HideAndSeekImp4 = OptionPlayerItem.Create(2, BetterSettingsTab, "BetterSetting.TempSetting.HideAndSeekImpNum", BetterGameSettings.HideAndSeekImpNum);
                    BetterGameSettingsTemp.HideAndSeekImp5 = OptionPlayerItem.Create(3, BetterSettingsTab, "BetterSetting.TempSetting.HideAndSeekImpNum", BetterGameSettings.HideAndSeekImpNum);
                }
            }
        }

        /*
        new BetterOptionCheckboxItem().Create(10000, BetterSettingsTab, "CheckBox Test", true);
        new BetterOptionStringItem().Create(10001, BetterSettingsTab, "String Test", ["Test 1", "Test 2", "Test 3"], 0);
        new BetterOptionFloatItem().Create(10002, BetterSettingsTab, "Float Test 1", [0f, 180f, 2.5f], 0f, "", "");
        new BetterOptionIntItem().Create(10003, BetterSettingsTab, "Int Test", [0, 5, 1], 0, "", "");
        new BetterOptionHeaderItem().Create(BetterSettingsTab, "<color=#4f92ff>Test Settings 2</color>");
        */
    }

    [HarmonyPatch(nameof(GameSettingMenu.Start))]
    [HarmonyPostfix]
    internal static void Start_Postfix(GameSettingMenu __instance)
    {
        SetupSettings();

        __instance.gameObject.transform.SetLocalY(-0.1f);
        GameObject PanelSprite = __instance.gameObject.transform.Find("PanelSprite").gameObject;
        if (PanelSprite != null)
        {
            PanelSprite.transform.SetLocalY(-0.32f);
            PanelSprite.transform.localScale = new Vector3(PanelSprite.transform.localScale.x, 0.625f);
        }

        __instance.GamePresetsButton.OnMouseOver.RemoveAllListeners();
        __instance.GameSettingsButton.OnMouseOver.RemoveAllListeners();
        __instance.RoleSettingsButton.OnMouseOver.RemoveAllListeners();


        BetterSettingsTab.TabButton.transform.localPosition = BetterSettingsTab.TabButton.transform.localPosition - new Vector3(0f, 1.265f, 0f);
        if (!GameState.IsHideNSeek && GameState.IsHost)
        {
            __instance.ChangeTab(1, false);
        }
        else if (GameState.IsHost)
        {
            __instance.RoleSettingsButton.gameObject.SetActive(true);
            __instance.RoleSettingsButton.GetComponent<PassiveButton>().enabled = false;
            __instance.RoleSettingsButton.inactiveSprites.GetComponent<SpriteRenderer>().color = new(0.5f, 0.5f, 0.5f, 1f);
            __instance.ChangeTab(1, false);
        }
        else
        {
            __instance.GamePresetsButton.GetComponent<PassiveButton>().enabled = false;
            __instance.GameSettingsButton.GetComponent<PassiveButton>().enabled = false;
            __instance.RoleSettingsButton.GetComponent<PassiveButton>().enabled = false;
            __instance.GamePresetsButton.inactiveSprites.GetComponent<SpriteRenderer>().color = new(0.5f, 0.5f, 0.5f, 1f);
            __instance.GameSettingsButton.inactiveSprites.GetComponent<SpriteRenderer>().color = new(0.5f, 0.5f, 0.5f, 1f);
            __instance.RoleSettingsButton.inactiveSprites.GetComponent<SpriteRenderer>().color = new(0.5f, 0.5f, 0.5f, 1f);
            __instance.ChangeTab(3, false);
        }
    }

    [HarmonyPatch(nameof(GameSettingMenu.ChangeTab))]
    [HarmonyPrefix]
    internal static void ChangeTab_Prefix(GameSettingMenu __instance, [HarmonyArgument(0)] int tabNum, [HarmonyArgument(1)] bool previewOnly)
    {
        if (BetterSettingsTab == null) return;

        BetterSettingsTab.AUTab.gameObject.SetActive(false);
        BetterSettingsTab.TabButton?.SelectButton(false);

        if (previewOnly && Controller.currentTouchType == Controller.TouchType.Joystick || !previewOnly)
        {
            switch (tabNum)
            {
                case 3:
                    BetterSettingsTab.AUTab.gameObject.SetActive(true);
                    BetterSettingsTab.TabButton?.SelectButton(true);
                    __instance.MenuDescriptionText.text = BetterSettingsTab.Description;
                    break;
            }
        }
    }
}

[HarmonyPatch(typeof(GameOptionsMenu))]
static class GameOptionsMenuPatch
{
    [HarmonyPatch(nameof(GameOptionsMenu.CreateSettings))]
    [HarmonyPrefix]
    internal static bool CreateSettings_Prefix(GameOptionsMenu __instance)
    {
        if (__instance == GameSettingMenuPatch.BetterSettingsTab.AUTab)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(OptionsConsole))]
static class OptionsConsolePatch
{
    [HarmonyPatch(nameof(OptionsConsole.CanUse))]
    [HarmonyPrefix]
    internal static void CanUse_Prefix(OptionsConsole __instance)
    {
        __instance.HostOnly = false;
    }
}

// Allow settings bypass
[HarmonyPatch(typeof(NumberOption))]
static class NumberOptionPatch
{
    [HarmonyPatch(nameof(NumberOption.Increase))]
    [HarmonyPrefix]
    internal static bool Increase_Prefix(NumberOption __instance)
    {
        int times = 1;
        if (Input.GetKey(KeyCode.LeftShift))
            times = 5;
        if (Input.GetKey(KeyCode.LeftControl))
            times = 10;

        if (__instance.Value + __instance.Increment * times > __instance.ValidRange.max)
        {
            __instance.Value = __instance.ValidRange.max;
        }
        else
        {
            __instance.Value = __instance.ValidRange.Clamp(__instance.Value + __instance.Increment * times);
        }
        __instance.UpdateValue();
        __instance.OnValueChanged.Invoke(__instance);
        __instance.AdjustButtonsActiveState();
        return false;
    }

    [HarmonyPatch(nameof(NumberOption.Decrease))]
    [HarmonyPrefix]
    internal static bool Decrease_Prefix(NumberOption __instance)
    {
        int times = 1;
        if (Input.GetKey(KeyCode.LeftShift))
            times = 5;
        if (Input.GetKey(KeyCode.LeftControl))
            times = 10;

        if (__instance.Value - __instance.Increment * times < __instance.ValidRange.min)
        {
            __instance.Value = __instance.ValidRange.min;
        }
        else
        {
            __instance.Value = __instance.ValidRange.Clamp(__instance.Value - __instance.Increment * times);
        }
        __instance.UpdateValue();
        __instance.OnValueChanged.Invoke(__instance);
        __instance.AdjustButtonsActiveState();
        return false;
    }
}