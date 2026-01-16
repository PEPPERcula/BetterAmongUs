using BepInEx.Configuration;
using BetterAmongUs.Helpers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BetterAmongUs.Modules;

internal sealed class ClientOptionItem
{
    internal ConfigEntry<bool>? Config;
    internal ToggleButtonBehaviour? ToggleButton;

    internal static SpriteRenderer? CustomBackground;
    private static List<ToggleButtonBehaviour>? OptionButtons;

    internal ClientOptionItem(string name, ConfigEntry<bool>? config, OptionsMenuBehaviour optionsMenuBehaviour, Action? additionalOnClickAction = null, Func<bool>? toggleCheck = null, bool IsToggle = true)
    {
        try
        {
            Config = config;

            var mouseMoveToggle = optionsMenuBehaviour.DisableMouseMovement;

            // 1つ目のボタンの生成時に背景も生成
            if (CustomBackground == null)
            {
                CustomBackground = Object.Instantiate(optionsMenuBehaviour.Background, optionsMenuBehaviour.transform);
                CustomBackground.name = "CustomBackground";
                CustomBackground.transform.localScale = new(0.9f, 0.9f, 1f);
                CustomBackground.transform.localPosition += Vector3.back * 8;
                CustomBackground.size += new Vector2(3f, 0f);
                CustomBackground.gameObject.SetActive(false);

                var closeButton = Object.Instantiate(mouseMoveToggle, CustomBackground.transform);
                closeButton.transform.localPosition = new(2.6f, -2.3f, -6f);
                closeButton.name = "Back";
                closeButton.Text.text = "Back";
                closeButton.Background.color = Palette.DisabledGrey;
                var closePassiveButton = closeButton.GetComponent<PassiveButton>();
                closePassiveButton.OnClick = new();
                closePassiveButton.OnClick.AddListener(new Action(() =>
                {
                    CustomBackground.gameObject.SetActive(false);
                }));

                var selectableButtons = optionsMenuBehaviour.ControllerSelectable;
                PassiveButton? leaveButton = null;
                PassiveButton? returnButton = null;
                foreach (var button in selectableButtons)
                {
                    if (button == null) continue;

                    if (button.name == "LeaveGameButton")
                        leaveButton = button.GetComponent<PassiveButton>();
                    else if (button.name == "ReturnToGameButton")
                        returnButton = button.GetComponent<PassiveButton>();
                }
                var generalTab = mouseMoveToggle.transform.parent.parent.parent;

                var modOptionsButton = Object.Instantiate(mouseMoveToggle, generalTab);
                modOptionsButton.transform.localPosition = leaveButton?.transform?.localPosition ?? new(0f, -2.4f, 1f);
                modOptionsButton.name = "Better Options";
                modOptionsButton.Text.text = Translator.GetString("BetterOption");
                modOptionsButton.Background.color = new Color32(0, 150, 0, byte.MaxValue);
                var modOptionsPassiveButton = modOptionsButton.GetComponent<PassiveButton>();
                modOptionsPassiveButton.OnClick = new();
                modOptionsPassiveButton.OnClick.AddListener(new Action(() =>
                {
                    AdjustButtonPositions();
                    CustomBackground.gameObject.SetActive(true);
                }));

                if (leaveButton != null)
                    leaveButton.transform.localPosition = new(-1.35f, -2.411f, -1f);
                if (returnButton != null)
                    returnButton.transform.localPosition = new(1.35f, -2.411f, -1f);

                OptionButtons = [];
            }

            ToggleButton = Object.Instantiate(mouseMoveToggle, CustomBackground.transform);
            OptionButtons.Add(ToggleButton);
            ToggleButton.transform.localPosition = new Vector3(
                (OptionButtons.Count - 1) % 3 == 0 ? -2.6f : ((OptionButtons.Count - 1) % 3 == 1 ? 0f : 2.6f),
                           2.2f - (0.5f * ((OptionButtons.Count - 1) / 3)),
                           -6f);
            ToggleButton.name = name;
            ToggleButton.Text.text = name;
            ToggleButton.Text.text += Config != null && Config.Value ? ": On" : ": Off";

            var passiveButton = ToggleButton.GetComponent<PassiveButton>();
            passiveButton.OnClick = new();

            if (IsToggle == false)
            {
                ToggleButton.Text.text = name;
                ToggleButton.Rollover?.ChangeOutColor(new(0, 150, 0, byte.MaxValue));
                ToggleButton.Text.color = new(1f, 1f, 1f, 1f);

                passiveButton.OnClick.AddListener(new Action(() =>
                {
                    if (toggleCheck() == false)
                    {
                        return;
                    }

                    additionalOnClickAction?.Invoke();
                }));

                return;
            }

            passiveButton.OnClick.AddListener(new Action(() =>
            {
                if (toggleCheck() == false)
                {
                    return;
                }

                if (config != null) config.Value = !config.Value;
                UpdateToggle();
                additionalOnClickAction?.Invoke();
            }));
            UpdateToggle();
        }
        catch (Exception ex)
        {
            Logger_.Error(ex.ToString(), "ClientOptionItem.Create");
        }
    }

    internal static ClientOptionItem Create(string name, ConfigEntry<bool> config, OptionsMenuBehaviour optionsMenuBehaviour, Action? additionalOnClickAction = null, Func<bool>? toggleCheck = null, bool IsToggle = true)
    {
        toggleCheck ??= () => true;

        return new ClientOptionItem(name, config, optionsMenuBehaviour, additionalOnClickAction, toggleCheck, IsToggle);
    }

    private static void AdjustButtonPositions()
    {
        if (OptionButtons == null || OptionButtons.Count == 0) return;

        int totalRows = (OptionButtons.Count + 2) / 3;

        float topPosition = 2.2f;
        float bottomLimit = -1.6f;
        float availableHeight = topPosition - bottomLimit;
        float rowSpacing = totalRows > 1 ? availableHeight / (totalRows - 1) : 0f;

        for (int i = 0; i < OptionButtons.Count; i++)
        {
            var button = OptionButtons[i];
            if (button == null) continue;

            int row = i / 3;
            int col = i % 3;
            float xPos = col == 0 ? -2.6f : (col == 1 ? 0f : 2.6f);
            float yPos = topPosition - (row * rowSpacing);

            button.transform.localPosition = new Vector3(xPos, yPos, -6f);
        }
    }

    internal void UpdateToggle()
    {
        if (ToggleButton == null) return;

        var color = Config != null && Config.Value ? new Color32(0, 150, 0, byte.MaxValue) : new Color32(77, 77, 77, byte.MaxValue);
        var textColor = Config != null && Config.Value ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, 0.5f);
        ToggleButton.Background.color = color;
        ToggleButton.Rollover?.ChangeOutColor(color);
        ToggleButton.Text.color = textColor;
        ToggleButton.Text.text = ToggleButton.name;
        ToggleButton.Text.text += Config != null && Config.Value ? ": On" : ": Off";
    }
}
