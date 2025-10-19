using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using UnityEngine;

namespace BetterAmongUs.Items.OptionItems;

/// <summary>
/// Int option.
/// Type: Int
/// </summary>
internal class OptionPlayerItem : OptionItem<int>
{
    internal override bool ShowChildren => base.ShowChildren && Value > Min;
    private int Max => BAUPlugin.AllPlayerControls.Count - 1;
    private int Min => CanBeRandom ? -1 : 0;
    protected bool CanBeRandom { get; set; }
    internal override bool CanLoad => false;

    private static List<OptionPlayerItem> optionPlayerItems = [];

    internal static OptionPlayerItem Create(int id, OptionTab tab, string tranStr, OptionItem parent = null, bool canBeRandom = true)
    {
        if (optionPlayerItems.FirstOrDefault(opt => opt.Id == id) is OptionPlayerItem playerItem)
        {
            playerItem.CreateBehavior();
            return playerItem;
        }

        OptionPlayerItem Item = new();
        optionPlayerItems.Add(Item);
        Item.Value = canBeRandom ? -1 : 0; ;
        Item._id = id;
        Item.Tab = tab;
        Item.Translation = tranStr;
        Item.CanBeRandom = canBeRandom;

        if (parent != null)
        {
            Item.Parent = parent;
            parent.Children.Add(Item);
        }

        Item.CreateBehavior();
        return Item;
    }

    internal override void Save()
    {
    }

    protected override void CreateBehavior()
    {
        if (!GameSettingMenu.Instance) return;
        AllTBROptionsTemp.Add(this);
        var numberOption = UnityEngine.Object.Instantiate(Tab.AUTab.numberOptionOrigin, Tab.AUTab.settingsContainer);
        Option = numberOption;
        Obj = Option.gameObject;
        Option.enabled = false;
        Tab.Children.Add(this);
        TitleTMP = numberOption.TitleText;
        ValueTMP = numberOption.ValueText;
        SetupText(numberOption.TitleText);
        SetupOptionBehavior();
        SetOptionVisuals();
    }

    protected override void SetupOptionBehavior()
    {
        if (Option is NumberOption numberOption)
        {
            SetupAUOption(Option);
            numberOption.DestroyTextTranslators();
            numberOption.TitleText.text = Name;
            numberOption.PlusBtn.OnClick = new();
            numberOption.PlusBtn.OnClick.AddListener((Action)(() => Increase()));
            numberOption.MinusBtn.OnClick = new();
            numberOption.MinusBtn.OnClick.AddListener((Action)(() => Decrease()));
        }
    }

    private void Increase()
    {
        int plus = 1;
        if (Input.GetKey(KeyCode.LeftShift))
            plus = 5;
        if (Input.GetKey(KeyCode.LeftControl))
            plus = 10;
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl))
            plus = 25;
        var value = Value;
        value += 1 * plus;
        SetValue(value);
    }

    private void Decrease()
    {
        int plus = 1;
        if (Input.GetKey(KeyCode.LeftShift))
            plus = 5;
        if (Input.GetKey(KeyCode.LeftControl))
            plus = 10;
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl))
            plus = 25;
        var value = Value;
        value -= 1 * plus;
        SetValue(value);
    }

    internal override void SetValue(int newValue)
    {
        newValue = Math.Clamp(newValue, Min, Max);
        base.SetValue(newValue);
    }

    internal static void ResetAllValues()
    {
        foreach (var opt in optionPlayerItems)
        {
            opt.ResetValue();
        }
    }

    internal static void UpdateAllValues()
    {
        foreach (var opt in optionPlayerItems)
        {
            opt.UpdateValue();
        }
    }

    internal void UpdateValue()
    {
        Value = Math.Clamp(Value, Min, Max);
        UpdateVisuals();
    }

    internal void ResetValue()
    {
        Value = Min;
    }

    internal override void UpdateVisuals(bool updateTabVisuals = true)
    {
        if (!GameSettingMenu.Instance) return;

        if (Option is NumberOption numberOption)
        {
            numberOption.PlusBtn.SetInteractable(false);
            numberOption.MinusBtn.SetInteractable(false);

            if (Value < Max)
            {
                numberOption.PlusBtn.SetInteractable(true);
            }
            if (Value > Min)
            {
                numberOption.MinusBtn.SetInteractable(true);
            }

            numberOption.ValueText.text = ValueAsString().Replace(InfiniteIcon, InfiniteIcon.Size(200f));
        }

        if (updateTabVisuals)
        {
            Tab.UpdateVisuals();
        }
    }

    internal override string ValueAsString()
    {
        if (Value != -1)
        {
            var player = Utils.PlayerFromPlayerId(Value);
            if (player != null)
                return $"{player.GetPlayerNameAndColor()}";
            else
                return "???";
        }
        else
        {
            return Translator.GetString(StringNames.RoundRobin).ToColor(Color.gray);
        }
    }

    internal override int GetInt() => GetValue();
    internal override float GetFloat() => GetValue();
    internal override bool Is(int @int) => @int == GetInt();
    internal override bool Is(float @float) => @float == GetFloat();
}
