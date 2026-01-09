using BetterAmongUs.Helpers;
using UnityEngine;

namespace BetterAmongUs.Items.OptionItems;

internal sealed class OptionIntItem : OptionItem<int>
{
    internal sealed override bool ShowChildren => base.ShowChildren && Value > 0;
    protected IntRange Range { get; set; }
    protected int Increment { get; set; }
    protected bool CanBeInfinite { get; set; }
    protected (string prefix, string postfix) Fixs { get; set; }

    internal static OptionIntItem Create(int id, OptionTab tab, string tranStr, (int minValue, int maxValue, int incrementValue) Min_Max_Increment, int defaultValue, (string prefix, string postfix) Prefix_Postfix = new(), OptionItem parent = null, bool canBeInfinite = false)
    {
        if (GetOptionById(id) is OptionIntItem intItem)
        {
            intItem.CreateBehavior();
            return intItem;
        }

        OptionIntItem Item = new();
        AllTBROptions.Add(Item);
        Item._id = id;
        Item.Tab = tab;
        Item.Translation = tranStr;
        Item.Increment = Min_Max_Increment.incrementValue;
        Item.CanBeInfinite = canBeInfinite;
        Item.Range = new IntRange(Min_Max_Increment.minValue, Min_Max_Increment.maxValue);
        Item.DefaultValue = defaultValue;
        Item.Fixs = Prefix_Postfix;

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

    protected sealed override void SetupOptionBehavior()
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
        value += Increment * plus;
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
        value -= Increment * plus;
        SetValue(value);
    }

    internal sealed override void SetValue(int newValue)
    {
        newValue = Math.Clamp(newValue, Range.min, Range.max);
        base.SetValue(newValue);
    }

    internal sealed override void UpdateVisuals(bool updateTabVisuals = true)
    {
        if (Option is NumberOption numberOption)
        {
            numberOption.PlusBtn.SetInteractable(false);
            numberOption.MinusBtn.SetInteractable(false);

            if (Value < Range.max)
            {
                numberOption.PlusBtn.SetInteractable(true);
            }
            if (Value > Range.min)
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

    internal sealed override string ValueAsString()
    {
        if (CanBeInfinite)
        {
            if (Value <= 0)
            {
                return InfiniteIcon;
            }
        }

        return $"{Fixs.prefix}{Value}{Fixs.postfix}";
    }

    internal sealed override int GetInt() => GetValue();
    internal sealed override float GetFloat() => GetValue();
    internal sealed override bool Is(int @int) => @int == GetInt();
    internal sealed override bool Is(float @float) => @float == GetFloat();
}
