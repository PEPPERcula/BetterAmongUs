using AmongUs.GameOptions;
using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches;


class BetterGameSettings
{
    public static BetterOptionItem? WhenCheating;
    public static BetterOptionItem? InvalidFriendCode;
    public static BetterOptionItem? UseBanPlayerList;
    public static BetterOptionItem? UseBanNameList;
    public static BetterOptionItem? UseBanWordList;
    public static BetterOptionItem? HideAndSeekImpNum;
    public static BetterOptionItem? DetectedLevelAbove;
    public static BetterOptionItem? DetectCheatClients;
    public static BetterOptionItem? DetectInvalidRPCs;

    public static BetterOptionItem? ExperimentalDetectInvalidSabotage;
}

[HarmonyPatch(typeof(GameSettingMenu))]
static class GameSettingMenuPatch
{
    private static PassiveButton BetterSettingsButton;
    public static GameOptionsMenu BetterSettingsTab;
    private static List<BetterOptionItem> TitleList = [];

    public static void SetupSettings(bool IsPreload = false)
    {
        BetterOptionItem.SpacingNum = 0;
        BetterOptionItem.BetterOptionItems.Clear();
        TitleList.Clear();

        // Anti-Cheat Settings
        {
            TitleList.Add(new BetterOptionHeaderItem().Create(BetterSettingsTab, "<color=#4f92ff>Anti-Cheat Settings</color>"));

            if (IsPreload || GameStates.IsHost)
            {
                TitleList.Add(new BetterOptionTitleItem().Create(BetterSettingsTab, $"<color=#4f92ff>Host Only</color>"));
                BetterGameSettings.WhenCheating = new BetterOptionStringItem().Create(100, BetterSettingsTab, "When a player is caught cheating", ["Do Nothing", "Kick", "Ban"], 2);
                BetterGameSettings.InvalidFriendCode = new BetterOptionCheckboxItem().Create(200, BetterSettingsTab, "Allow invalid friendCodes", true);
                BetterGameSettings.UseBanPlayerList = new BetterOptionCheckboxItem().Create(300, BetterSettingsTab, "Use Ban Player List", true);
                BetterGameSettings.UseBanNameList = new BetterOptionCheckboxItem().Create(400, BetterSettingsTab, "Use Ban Name List", true);
                BetterGameSettings.UseBanWordList = new BetterOptionCheckboxItem().Create(500, BetterSettingsTab, "Use Ban Word List", true);
                TitleList.Add(new BetterOptionDividerItem().Create(BetterSettingsTab));
            }

            TitleList.Add(new BetterOptionTitleItem().Create(BetterSettingsTab, $"<color=#4f92ff>Detections</color>"));
            BetterGameSettings.DetectedLevelAbove = new BetterOptionIntItem().Create(600, BetterSettingsTab, "Detected player level = or >", [100, 5000, 5], 200, "Lv ", "");
            BetterGameSettings.DetectCheatClients = new BetterOptionCheckboxItem().Create(700, BetterSettingsTab, "Detect Cheat Clients", true);
            BetterGameSettings.DetectInvalidRPCs = new BetterOptionCheckboxItem().Create(800, BetterSettingsTab, "Detect Invalid RPCs", true);

            TitleList.Add(new BetterOptionDividerItem().Create(BetterSettingsTab));
            TitleList.Add(new BetterOptionTitleItem().Create(BetterSettingsTab, $"<color=#f20>Experimental</color>"));
            BetterGameSettings.ExperimentalDetectInvalidSabotage = new BetterOptionCheckboxItem().Create(100000, BetterSettingsTab, "Detect Invalid Sabotages", false);
        }

        // Gameplay Settings
        {
            if (IsPreload || GameStates.IsHost)
            {
                if (IsPreload || !GameStates.IsHideNSeek)
                {
                    new BetterOptionHeaderItem().Create(BetterSettingsTab, "<color=#d7d700>Gameplay Settings</color>");
                }
                else if (IsPreload || GameStates.IsHideNSeek)
                {
                    new BetterOptionHeaderItem().Create(BetterSettingsTab, "<color=#d7d700>Hide & Seek Settings</color>");

                    if (IsPreload || Main.BetterRoleAlgorithma.Value)
                    {
                        new BetterOptionTitleItem().Create(BetterSettingsTab, $"<color=#4f92ff>Better Role Algorithma</color>");
                        BetterOptionItem.SpacingNum += 0.2f;
                        new BetterOptionTitleItem().Create(BetterSettingsTab, $"<color={Utils.GetRoleColor(RoleTypes.Impostor)}>Imposter</color>");
                        BetterGameSettings.HideAndSeekImpNum = new BetterOptionIntItem().Create(1000, BetterSettingsTab, "# Seekers", [1, 5, 1], 1, "", "");
                    }
                }
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
            BetterSettingsTab.scrollBar.SetYBoundsMax(1.25f * BetterOptionItem.SpacingNum / 2);
    }

    private static void Initialize()
    {
        _ = new LateTask(() =>
        {
            foreach (var item in BetterOptionItem.BetterOptionItems)
            {
                if (item != null)
                {
                    if (item.TitleText != null)
                    {
                        item.TitleText.text = item.Name;
                    }
                }
            }
        }, 0.005f, shoudLog: false);
    }

    [HarmonyPatch(nameof(GameSettingMenu.Update))]
    [HarmonyPostfix]
    public static void Update_Postfix(GameSettingMenu __instance)
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
            if (BetterSettingsButton.selected)
            {
                __instance.MenuDescriptionText.text = "Edit better settings for your lobby and gameplay.";
            }
        }
    }

    [HarmonyPatch(nameof(GameSettingMenu.Start))]
    [HarmonyPostfix]
    public static void Start_Postfix(GameSettingMenu __instance)
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
        }));

        BetterSettingsTab = UnityEngine.Object.Instantiate(__instance.GameSettingsTab, __instance.GameSettingsTab.transform.parent);
        BetterSettingsTab.name = "BETTER SETTINGS TAB";
        BetterSettingsTab.scrollBar.Inner.DestroyChildren();

        __instance.GamePresetsButton.OnMouseOver.RemoveAllListeners();
        __instance.GameSettingsButton.OnMouseOver.RemoveAllListeners();
        __instance.RoleSettingsButton.OnMouseOver.RemoveAllListeners();


        BetterSettingsButton.transform.position = BetterSettingsButton.transform.position - new Vector3(0f, 1.265f, 0f);
        if (!GameStates.IsHideNSeek && GameStates.IsHost)
        {
            __instance.ChangeTab(1, false);
        }
        else if (GameStates.IsHost)
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

[HarmonyPatch(typeof(GameOptionsMenu))]
static class GameOptionsMenuPatch
{
    [HarmonyPatch(nameof(GameOptionsMenu.CreateSettings))]
    [HarmonyPrefix]
    public static bool CreateSettings_Prefix(GameOptionsMenu __instance)
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
    public static void CanUse_Prefix(OptionsConsole __instance)
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
    public static bool Increase_Prefix(NumberOption __instance)
    {
        int times = 1;
        if (Input.GetKey(KeyCode.LeftShift))
            times = 5;
        if (Input.GetKey(KeyCode.LeftControl))
            times = 10;

        __instance.Value += __instance.Increment * times;
        __instance.UpdateValue();
        return false;
    }

    [HarmonyPatch(nameof(NumberOption.Decrease))]
    [HarmonyPrefix]
    public static bool Decrease_Prefix(NumberOption __instance)
    {
        int times = 1;
        if (Input.GetKey(KeyCode.LeftShift))
            times = 5;
        if (Input.GetKey(KeyCode.LeftControl))
            times = 10;

        __instance.Value -= __instance.Increment * times;
        __instance.UpdateValue();
        return false;
    }

    [HarmonyPatch(nameof(NumberOption.UpdateValue))]
    [HarmonyPrefix]
    public static bool UpdateValue_Prefix(NumberOption __instance)
    {
        if (__instance.floatOptionName != FloatOptionNames.Invalid)
        {
            GameOptionsManager.Instance.CurrentGameOptions.SetFloat(__instance.floatOptionName, __instance.GetFloat());
            return false;
        }
        if (__instance.intOptionName != Int32OptionNames.Invalid)
        {
            GameOptionsManager.Instance.CurrentGameOptions.SetInt(__instance.intOptionName, __instance.GetInt());
            return false;
        }
        return false;
    }

    [HarmonyPatch(nameof(NumberOption.FixedUpdate))]
    [HarmonyPrefix]
    public static bool FixedUpdate_Prefix(NumberOption __instance)
    {
        try
        {
            if (__instance.MinusBtn != null && __instance.PlusBtn != null)
            {
                __instance.MinusBtn.SetInteractable(true);
                __instance.PlusBtn.SetInteractable(true);
            }

            if (__instance.oldValue != __instance.Value)
            {
                __instance.oldValue = __instance.Value;

                if (__instance.Value > __instance.ValidRange.max || __instance.Value < __instance.ValidRange.min)
                {
                    __instance.ValueText.text = $"<color=#f20>{__instance.data.GetValueString(__instance.Value)}</color>";
                }
                else
                {
                    __instance.ValueText.text = __instance.data.GetValueString(__instance.Value);
                }
            }
        }
        catch { }

        return false;
    }
}