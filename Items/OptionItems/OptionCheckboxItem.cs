using BetterAmongUs.Helpers;
using UnityEngine;

namespace BetterAmongUs.Items.OptionItems;

internal class OptionCheckboxItem : OptionItem<bool>
{
    internal override bool ShowChildren => base.ShowChildren && Value;

    /// <summary>
    /// Creates a new checkbox item for the options menu. If an item with the specified ID already exists, 
    /// it reuses the existing item and sets up its behavior.
    /// </summary>
    /// <param name="id">The unique identifier for the checkbox item.</param>
    /// <param name="tab">The tab to which the checkbox item belongs.</param>
    /// <param name="tranStr">The translation string for the checkbox item label.</param>
    /// <param name="defaultValue">The default value (checked/unchecked) for the checkbox.</param>
    /// <param name="parent">An optional parent option item that this checkbox item belongs to.</param>
    /// <param name="vanillaOption">An optional vanilla option name, if any, for this checkbox item.</param>
    /// <returns>The created or reused <see cref="OptionCheckboxItem"/> instance.</returns>
    internal static OptionCheckboxItem Create(int id, OptionTab tab, string tranStr, bool defaultValue, OptionItem parent = null)
    {
        if (GetOptionById(id) is OptionCheckboxItem checkboxItem)
        {
            checkboxItem.CreateBehavior();
            return checkboxItem;
        }

        OptionCheckboxItem Item = new();
        AllTBROptions.Add(Item);
        Item._id = id;
        Item.Tab = tab;
        Item.Translation = tranStr;
        Item.DefaultValue = defaultValue;

        if (parent != null)
        {
            Item.Parent = parent;
            parent.Children.Add(Item);
        }

        Item.CreateBehavior();
        return Item;
    }

    protected override void CreateBehavior()
    {
        TryLoad();
        if (!GameSettingMenu.Instance) return;
        AllTBROptionsTemp.Add(this);
        var ToggleOption = UnityEngine.Object.Instantiate(Tab.AUTab.checkboxOrigin, Tab.AUTab.settingsContainer);
        Option = ToggleOption;
        Obj = Option.gameObject;
        Option.enabled = false;
        Tab.Children.Add(this);
        TitleTMP = ToggleOption.TitleText;
        SetupText(ToggleOption.TitleText);
        SetupOptionBehavior();
        SetOptionVisuals();
    }

    protected override void SetupOptionBehavior()
    {
        if (Option is ToggleOption toggleOption)
        {
            SetupAUOption(Option);
            toggleOption.DestroyTextTranslators();
            toggleOption.TitleText.text = Name;
            var button = toggleOption.buttons[0];
            button.OnClick = new();
            button.OnClick.AddListener((Action)(() => SetValue(!Value)));
        }
    }

    internal override void UpdateVisuals(bool updateTabVisuals = true)
    {
        if (Option is ToggleOption toggleOption)
        {
            toggleOption.CheckMark.enabled = Value;
        }

        if (updateTabVisuals)
        {
            Tab.UpdateVisuals();
        }
    }

    internal override string ValueAsString()
    {
        Color color = Value ? Color.green : Color.red;
        string @bool = Value ? "On" : "Off";
        return $"<color={Colors.Color32ToHex(color)}>{@bool}</color>";
    }

    internal override bool GetBool() => GetValue();
    internal override bool Is(bool @bool) => @bool == GetBool();
}
