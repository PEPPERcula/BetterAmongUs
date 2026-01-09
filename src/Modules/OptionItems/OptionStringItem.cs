using BetterAmongUs.Helpers;

namespace BetterAmongUs.Modules.OptionItems;

internal sealed class OptionStringItem : OptionItem<int>
{
    private IntRange Range { get; set; } = new();
    private string[] TranslatorStrings { get; set; } = [];
    private bool CanBeRandom { get; set; }

    internal static OptionStringItem Create(int id, OptionTab tab, string tranStr, string[] tranStrings, int defaultValue, OptionItem? parent = null, bool canBeRandom = false)
    {
        if (tranStrings.Length < 2)
        {
            throw new ArgumentException("tranStrings must have more then 1 string!");
        }

        if (GetOptionById(id) is OptionStringItem stringItem)
        {
            stringItem.CreateBehavior();
            return stringItem;
        }

        if (defaultValue < 0)
        {
            canBeRandom = true;
        }

        OptionStringItem Item = new();
        AllTBROptions.Add(Item);
        Item._id = id;
        Item.Tab = tab;
        Item.Translation = tranStr;
        Item.TranslatorStrings = tranStrings;
        Item.Range = new IntRange(0, tranStrings.Length - 1);
        Item.DefaultValue = !canBeRandom ? defaultValue : defaultValue + 1;
        Item.CanBeRandom = canBeRandom;
        if (canBeRandom)
        {
            var list = Item.TranslatorStrings.ToList();
            list.Insert(0, "Option.RandomWithColor");
            Item.TranslatorStrings = [.. list];
            Item.Range = new IntRange(0, list.Count - 1);
        }

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
        SetValue(Value + 1);
    }

    private void Decrease()
    {
        SetValue(Value - 1);
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

            numberOption.ValueText.text = $"{ValueAsString()}";
        }

        if (updateTabVisuals)
        {
            Tab.UpdateVisuals();
        }
    }

    internal sealed override string ValueAsString() => Translator.GetString(TranslatorStrings[Value], showInvalid: false);
    internal sealed override int GetStringValue()
    {
        var value = GetValue();
        if (!CanBeRandom)
        {
            return value;
        }
        else
        {
            if (value == 0)
            {
                return TranslatorStrings.Skip(1).RandomIndex().index;
            }
            else
            {
                return value - 1;
            }
        }
    }

    internal sealed override bool Is(string @string) => TranslatorStrings[Value] == @string || ValueAsString() == @string;
    internal sealed override bool Is(int @int) => !CanBeRandom ? Value == @int : Value == @int - 1;
}
