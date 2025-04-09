using AmongUs.GameOptions;
using BetterAmongUs.Helpers;
using BetterAmongUs.Items.OptionItems;
using BetterAmongUs.Modules;
using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches;


class BetterGameSettings
{
    internal static BetterOptionItem? WhenCheating;
    internal static BetterOptionItem? InvalidFriendCode;
    internal static BetterOptionItem? UseBanPlayerList;
    internal static BetterOptionItem? UseBanNameList;
    internal static BetterOptionItem? UseBanWordList;
    internal static BetterOptionItem? UseBanWordListOnlyLobby;
    internal static BetterOptionItem? HideAndSeekImpNum;
    internal static BetterOptionItem? DetectedLevelAbove;
    internal static BetterOptionItem? DetectCheatClients;
    internal static BetterOptionItem? DetectInvalidRPCs;
    internal static BetterOptionItem? RoleRandomizer;
    internal static BetterOptionItem? DesyncRoles;
    internal static BetterOptionItem? CancelInvalidSabotage;
    internal static BetterOptionItem? CensorDetectionReason;
    internal static BetterOptionItem? RemovePetOnDeath;
    internal static BetterOptionItem? DisableSabotages;
}

class BetterGameSettingsTemp
{
    internal static BetterOptionItem? HideAndSeekImp2;
    internal static BetterOptionItem? HideAndSeekImp3;
    internal static BetterOptionItem? HideAndSeekImp4;
    internal static BetterOptionItem? HideAndSeekImp5;
}

[HarmonyPatch(typeof(GameSettingMenu))]
static class GameSettingMenuPatch
{
    private static PassiveButton BetterSettingsButton;
    internal static GameOptionsMenu BetterSettingsTab;
    private static List<BetterOptionItem> TitleList = [];

    internal static void SetupSettings(bool IsPreload = false)
    {
        BetterOptionItem.BetterOptionItems.Clear();
        BetterOptionItem.TempPlayerOptionDataNum = 0;
        TitleList.Clear();

        // Use 1700 next ID

        // Anti-Cheat Settings
        {
            TitleList.Add(new BetterOptionHeaderItem().Create(BetterSettingsTab, Translator.GetString("BetterSetting.MainHeader.AntiCheat")));

            if (IsPreload || GameState.IsHost)
            {
                TitleList.Add(new BetterOptionTitleItem().Create(BetterSettingsTab, Translator.GetString("BetterSetting.TextHeader.HostOnly")));
                BetterGameSettings.WhenCheating = new BetterOptionStringItem().Create(100, BetterSettingsTab, Translator.GetString("BetterSetting.Setting.WhenCheating"),
                    [Translator.GetString("BetterSetting.Setting.WhenCheating.Notify"),
                        Translator.GetString("BetterSetting.Setting.WhenCheating.Kick"),
                        Translator.GetString("BetterSetting.Setting.WhenCheating.Ban")], 2);
                BetterGameSettings.InvalidFriendCode = new BetterOptionCheckboxItem().Create(200, BetterSettingsTab, Translator.GetString("BetterSetting.Setting.InvalidFriendCode"), true);
                BetterGameSettings.CancelInvalidSabotage = new BetterOptionCheckboxItem().Create(900, BetterSettingsTab, Translator.GetString("BetterSetting.Setting.CancelInvalidSabotage"), true);
                BetterGameSettings.UseBanPlayerList = new BetterOptionCheckboxItem().Create(300, BetterSettingsTab, Translator.GetString("BetterSetting.Setting.UseBanPlayerList"), true);
                BetterGameSettings.UseBanNameList = new BetterOptionCheckboxItem().Create(400, BetterSettingsTab, Translator.GetString("BetterSetting.Setting.UseBanNameList"), true);
                BetterGameSettings.UseBanWordList = new BetterOptionCheckboxItem().Create(500, BetterSettingsTab, Translator.GetString("BetterSetting.Setting.UseBanWordList"), true);
                BetterGameSettings.UseBanWordListOnlyLobby = new BetterOptionCheckboxItem().Create(1400, BetterSettingsTab, Translator.GetString("BetterSetting.Setting.UseBanWordListOnlyLobby"), true, BetterGameSettings.UseBanWordList);
                TitleList.Add(new BetterOptionDividerItem().Create(BetterSettingsTab));
            }

            TitleList.Add(new BetterOptionTitleItem().Create(BetterSettingsTab, Translator.GetString("BetterSetting.TextHeader.Detections")));
            BetterGameSettings.CensorDetectionReason = new BetterOptionCheckboxItem().Create(1300, BetterSettingsTab, Translator.GetString("BetterSetting.Setting.CensorDetectionReason"), false);
            BetterGameSettings.DetectedLevelAbove = new BetterOptionIntItem().Create(600, BetterSettingsTab, Translator.GetString("BetterSetting.Setting.DetectedLevelAbove"), [100, 5000, 5], 200, "Lv ", "");
            BetterGameSettings.DetectCheatClients = new BetterOptionCheckboxItem().Create(700, BetterSettingsTab, Translator.GetString("BetterSetting.Setting.DetectCheatClients"), true);
            BetterGameSettings.DetectInvalidRPCs = new BetterOptionCheckboxItem().Create(800, BetterSettingsTab, Translator.GetString("BetterSetting.Setting.DetectInvalidRPCs"), true);

            /*
            TitleList.Add(new BetterOptionDividerItem().Create(BetterSettingsTab));
            TitleList.Add(new BetterOptionTitleItem().Create(BetterSettingsTab, $"<color=#f20>Experimental</color>"));
            BetterGameSettings.CancelInvalidSabotage = new BetterOptionCheckboxItem().Create(100000, BetterSettingsTab, "Detect Invalid Sabotages", false);
            */
        }

        if (IsPreload || GameState.IsHost)
        {
            TitleList.Add(new BetterOptionHeaderItem().Create(BetterSettingsTab, Translator.GetString("BetterSetting.MainHeader.RoleAlgorithm")));
            BetterGameSettings.RoleRandomizer = new BetterOptionStringItem().Create(1100, BetterSettingsTab, Translator.GetString("BetterSetting.Setting.RoleRandomizer"), ["System.Random", "UnityEngine.Random"], 0);
            BetterGameSettings.DesyncRoles = new BetterOptionCheckboxItem().Create(1200, BetterSettingsTab, Translator.GetString("BetterSetting.Setting.DesyncRoles"), true);
        }

        // Gameplay Settings
        {
            if (IsPreload || GameState.IsHost && GameState.IsPrivateOnlyLobby)
            {
                if (IsPreload || !GameState.IsHideNSeek)
                {
                    new BetterOptionHeaderItem().Create(BetterSettingsTab, Translator.GetString("BetterSetting.MainHeader.Gameplay"));
                    BetterGameSettings.DisableSabotages = new BetterOptionCheckboxItem().Create(1500, BetterSettingsTab, Translator.GetString("BetterSetting.Setting.DisableSabotages"), false);
                    BetterGameSettings.RemovePetOnDeath = new BetterOptionCheckboxItem().Create(1600, BetterSettingsTab, Translator.GetString("BetterSetting.Setting.RemovePetOnDeath"), false);
                }
                else if (IsPreload || GameState.IsHideNSeek)
                {
                    new BetterOptionHeaderItem().Create(BetterSettingsTab, Translator.GetString("BetterSetting.MainHeader.HideNSeek"));
                    new BetterOptionTitleItem().Create(BetterSettingsTab, $"<color={Utils.GetRoleColor(RoleTypes.Impostor)}>{Translator.GetString(StringNames.ImpostorsCategory)}</color>");
                    BetterGameSettings.HideAndSeekImpNum = new BetterOptionIntItem().Create(1000, BetterSettingsTab, Translator.GetString("BetterSetting.Setting.HideAndSeekImpNum"), [1, 5, 1], 1, "", "");
                    BetterGameSettingsTemp.HideAndSeekImp2 = new BetterOptionPlayerItem().Create(BetterSettingsTab, Translator.GetString("BetterSetting.TempSetting.HideAndSeekImpNum"), BetterGameSettings.HideAndSeekImpNum, new Func<bool>(() =>
                    {
                        return BetterGameSettings.HideAndSeekImpNum is BetterOptionIntItem betterOption && betterOption.CurrentValue > 1;
                    }));
                    BetterGameSettingsTemp.HideAndSeekImp3 = new BetterOptionPlayerItem().Create(BetterSettingsTab, Translator.GetString("BetterSetting.TempSetting.HideAndSeekImpNum"), BetterGameSettings.HideAndSeekImpNum, new Func<bool>(() =>
                    {
                        return BetterGameSettings.HideAndSeekImpNum is BetterOptionIntItem betterOption && betterOption.CurrentValue > 2
                            && BetterGameSettingsTemp.HideAndSeekImp2 is BetterOptionPlayerItem betterOption2 && betterOption2.CurrentIndex > -1;
                    }));
                    BetterGameSettingsTemp.HideAndSeekImp4 = new BetterOptionPlayerItem().Create(BetterSettingsTab, Translator.GetString("BetterSetting.TempSetting.HideAndSeekImpNum"), BetterGameSettings.HideAndSeekImpNum, new Func<bool>(() =>
                    {
                        return BetterGameSettings.HideAndSeekImpNum is BetterOptionIntItem betterOption && betterOption.CurrentValue > 3
                            && BetterGameSettingsTemp.HideAndSeekImp2 is BetterOptionPlayerItem betterOption2 && betterOption2.CurrentIndex > -1
                            && BetterGameSettingsTemp.HideAndSeekImp3 is BetterOptionPlayerItem betterOption3 && betterOption3.CurrentIndex > -1;
                    }));
                    BetterGameSettingsTemp.HideAndSeekImp5 = new BetterOptionPlayerItem().Create(BetterSettingsTab, Translator.GetString("BetterSetting.TempSetting.HideAndSeekImpNum"), BetterGameSettings.HideAndSeekImpNum, new Func<bool>(() =>
                    {
                        return BetterGameSettings.HideAndSeekImpNum is BetterOptionIntItem betterOption && betterOption.CurrentValue > 4
                            && BetterGameSettingsTemp.HideAndSeekImp2 is BetterOptionPlayerItem betterOption2 && betterOption2.CurrentIndex > -1
                            && BetterGameSettingsTemp.HideAndSeekImp3 is BetterOptionPlayerItem betterOption3 && betterOption3.CurrentIndex > -1
                            && BetterGameSettingsTemp.HideAndSeekImp4 is BetterOptionPlayerItem betterOption4 && betterOption4.CurrentIndex > -1;
                    }));
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

    private static void Initialize()
    {
        _ = new LateTask(() =>
        {
            foreach (var item in BetterOptionItem.BetterOptionItems)
            {
                if (item != null)
                {
                    item.obj.SetActive(true);

                    if (item.TitleText != null)
                    {
                        item.TitleText.text = item.Name;
                    }
                }
            }

            BetterOptionItem.UpdatePositions();
        }, 0.005f, shouldLog: false);
    }

    [HarmonyPatch(nameof(GameSettingMenu.Update))]
    [HarmonyPostfix]
    internal static void Update_Postfix(GameSettingMenu __instance)
    {
        if (BetterSettingsButton != null)
        {
            BetterSettingsButton.buttonText.SetText(Translator.GetString("BetterSetting"));

            if (!BetterSettingsButton.selected && !BetterSettingsButton.activeSprites.active)
            {
                BetterSettingsButton.buttonText.color = new Color(0f, 1f, 0f, 1f);
            }
            else
            {
                BetterSettingsButton.buttonText.color = new Color(0.35f, 1f, 0.35f, 1f);
            }
            if (BetterSettingsButton.selected)
            {
                __instance.MenuDescriptionText.text = Translator.GetString("BetterSetting.Description");
            }
        }
    }

    [HarmonyPatch(nameof(GameSettingMenu.Start))]
    [HarmonyPostfix]
    internal static void Start_Postfix(GameSettingMenu __instance)
    {
        __instance.gameObject.transform.SetLocalY(-0.1f);
        GameObject PanelSprite = __instance.gameObject.transform.Find("PanelSprite").gameObject;
        if (PanelSprite != null)
        {
            PanelSprite.transform.SetLocalY(-0.32f);
            PanelSprite.transform.localScale = new Vector3(PanelSprite.transform.localScale.x, 0.625f);
        }

        BetterSettingsButton = UnityEngine.Object.Instantiate(__instance.GameSettingsButton, __instance.GameSettingsButton.transform.parent);

        BetterSettingsButton.name = "BetterSettings";
        BetterSettingsButton.OnClick.RemoveAllListeners();
        BetterSettingsButton.OnMouseOver.RemoveAllListeners();

        BetterSettingsButton.activeSprites.GetComponent<SpriteRenderer>().color = new Color(0f, 1f, 0.35f, 1f);
        BetterSettingsButton.inactiveSprites.GetComponent<SpriteRenderer>().color = new Color(0f, 1f, 0.35f, 1f);
        BetterSettingsButton.selectedSprites.GetComponent<SpriteRenderer>().color = new Color(0f, 1f, 0.35f, 1f);


        BetterSettingsButton.OnClick.AddListener(new Action(() =>
        {
            __instance.ChangeTab(3, false);
            BetterOptionItem.UpdatePositions();
        }));

        BetterSettingsTab = UnityEngine.Object.Instantiate(__instance.GameSettingsTab, __instance.GameSettingsTab.transform.parent);
        BetterSettingsTab.name = "BETTER SETTINGS TAB";
        BetterSettingsTab.scrollBar.Inner.DestroyChildren();

        __instance.GamePresetsButton.OnMouseOver.RemoveAllListeners();
        __instance.GameSettingsButton.OnMouseOver.RemoveAllListeners();
        __instance.RoleSettingsButton.OnMouseOver.RemoveAllListeners();


        BetterSettingsButton.transform.localPosition = BetterSettingsButton.transform.localPosition - new Vector3(0f, 1.265f, 0f);
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

        SetupSettings();
    }

    [HarmonyPatch(nameof(GameSettingMenu.ChangeTab))]
    [HarmonyPrefix]
    internal static void ChangeTab_Prefix(GameSettingMenu __instance, [HarmonyArgument(0)] int tabNum, [HarmonyArgument(1)] bool previewOnly)
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
                    break;
            }
        }
    }

    [HarmonyPatch(nameof(GameSettingMenu.ChangeTab))]
    [HarmonyPostfix]
    internal static void ChangeTab_Postfix(GameSettingMenu __instance, [HarmonyArgument(0)] int tabNum)
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

[HarmonyPatch(typeof(GameOptionsMenu))]
static class GameOptionsMenuPatch
{
    [HarmonyPatch(nameof(GameOptionsMenu.CreateSettings))]
    [HarmonyPrefix]
    internal static bool CreateSettings_Prefix(GameOptionsMenu __instance)
    {
        if (__instance == GameSettingMenuPatch.BetterSettingsTab)
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