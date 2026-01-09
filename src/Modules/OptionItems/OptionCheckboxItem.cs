using BetterAmongUs.Helpers;
using UnityEngine;

namespace BetterAmongUs.Modules.OptionItems;

internal sealed class OptionCheckboxItem : OptionItem<bool>
{
    internal sealed override bool ShowChildren => base.ShowChildren && Value;

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

    protected sealed override void CreateBehavior()
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

    protected sealed override void SetupOptionBehavior()
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

    internal sealed override void UpdateVisuals(bool updateTabVisuals = true)
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

    internal sealed override string ValueAsString()
    {
        Color color = Value ? Color.green : Color.red;
        string @bool = Value ? "On" : "Off";
        return $"<color={Colors.Color32ToHex(color)}>{@bool}</color>";
    }

    internal sealed override bool GetBool() => GetValue();
    internal sealed override bool Is(bool @bool) => @bool == GetBool();
}
