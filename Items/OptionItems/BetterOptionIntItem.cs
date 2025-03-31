using BetterAmongUs.Data;
using UnityEngine;

namespace BetterAmongUs.Items.OptionItems;

internal class BetterOptionIntItem : BetterOptionItem
{
    private NumberOption? ThisOption;
    internal int CurrentValue;
    internal IntRange intRange = new(0, 180);
    internal int Increment = 1;
    private string? PostFix;
    private string? PreFix;

    internal override bool ShowChildrenCondition() => CurrentValue > intRange.min;
    internal override bool SelfShowCondition() => ShowCondition != null ? ShowCondition() : base.SelfShowCondition();
    internal Func<bool>? ShowCondition = null;

    internal BetterOptionItem Create(int id, GameOptionsMenu gameOptionsMenu, string name, int[] values, int DefaultValue, string preFix = "", string postFix = "", BetterOptionItem? Parent = null, Func<bool>? selfShowCondition = null)
    {
        Id = id;
        intRange = new(values[0], values[1]);
        Increment = values[2];
        if (DefaultValue < intRange.min) DefaultValue = intRange.min;
        if (DefaultValue > intRange.max) DefaultValue = intRange.max;
        CurrentValue = DefaultValue;
        ShowCondition = selfShowCondition;

        if (gameOptionsMenu == null)
        {
            Load(DefaultValue);
            return this;
        }

        if (values.Length is < 3 or > 3) return null;

        NumberOption optionBehaviour = UnityEngine.Object.Instantiate(gameOptionsMenu.numberOptionOrigin, Vector3.zero, Quaternion.identity, gameOptionsMenu.settingsContainer);
        optionBehaviour.transform.localPosition = new Vector3(0.952f, 2f, -2f);
        SetUp(optionBehaviour);
        optionBehaviour.OnValueChanged = new Action<OptionBehaviour>((option) => ValueChanged(id, option));

        optionBehaviour.PlusBtn.OnClick.RemoveAllListeners();
        optionBehaviour.MinusBtn.OnClick.RemoveAllListeners();
        optionBehaviour.PlusBtn.OnClick.AddListener(new Action(() => Increase()));
        optionBehaviour.MinusBtn.OnClick.AddListener(new Action(() => Decrease()));
        optionBehaviour.PlusBtn.OnClick.AddListener(new Action(() => ValueChanged(id, optionBehaviour)));
        optionBehaviour.MinusBtn.OnClick.AddListener(new Action(() => ValueChanged(id, optionBehaviour)));

        optionBehaviour.LabelBackground.transform.localScale = new Vector3(1.6f, 0.78f);
        optionBehaviour.LabelBackground.transform.SetLocalX(-2.4f);
        optionBehaviour.TitleText.enableAutoSizing = false;
        optionBehaviour.TitleText.transform.SetLocalX(-1.5f);
        optionBehaviour.TitleText.alignment = TMPro.TextAlignmentOptions.Right;
        optionBehaviour.TitleText.enableWordWrapping = false;
        optionBehaviour.TitleText.fontSize = 2.5f;

        // Set data
        Tab = gameOptionsMenu;
        Name = name;
        TitleText = optionBehaviour.TitleText;
        Option = optionBehaviour;
        PostFix = postFix;
        PreFix = preFix;
        ThisOption = optionBehaviour;

        Load(DefaultValue);
        AdjustButtonsActiveState();

        BetterOptionItems.Add(this);
        obj = optionBehaviour.gameObject;

        if (Parent != null)
        {
            int Index = 1;
            var tempParent = Parent;

            while (tempParent.ThisParent != null)
            {
                tempParent = tempParent.ThisParent;
                Index++;
            }

            optionBehaviour.LabelBackground.GetComponent<SpriteRenderer>().color -= new Color(0.25f, 0.25f, 0.25f, 0f) * Index;
            optionBehaviour.LabelBackground.transform.localScale -= new Vector3(0.04f, 0f, 0f) * Index;
            optionBehaviour.LabelBackground.transform.position += new Vector3(0.04f, 0f, 0f) * Index;
            optionBehaviour.LabelBackground.transform.SetLocalZ(1f);
            ThisParent = Parent;
            IsChild = true;
            Parent.ChildrenList.Add(this);
        }

        return this;
    }

    private void AdjustButtonsActiveState()
    {
        if (ThisOption == null) return;

        ThisOption.ValueText.text = PreFix + CurrentValue.ToString() + PostFix;

        if (CurrentValue + Increment > intRange.max)
        {
            ThisOption.PlusBtn.SetInteractable(false);
            ThisOption.MinusBtn.SetInteractable(true);
        }
        else if (CurrentValue - Increment < intRange.min)
        {
            ThisOption.PlusBtn.SetInteractable(true);
            ThisOption.MinusBtn.SetInteractable(false);
        }
        else
        {
            ThisOption.PlusBtn.SetInteractable(true);
            ThisOption.MinusBtn.SetInteractable(true);
        }

        BetterDataManager.SaveSetting(Id, CurrentValue);
    }

    internal void Increase()
    {
        int times = 1;
        if (Input.GetKey(KeyCode.LeftShift))
            times = 5;
        if (Input.GetKey(KeyCode.LeftControl))
            times = 10;

        if (CurrentValue + Increment * times <= intRange.max)
        {
            CurrentValue += Increment * times;
        }
        else
        {
            CurrentValue = intRange.max;
        }

        AdjustButtonsActiveState();
    }

    internal void Decrease()
    {
        int times = 1;
        if (Input.GetKey(KeyCode.LeftShift))
            times = 5;
        if (Input.GetKey(KeyCode.LeftControl))
            times = 10;

        if (CurrentValue - Increment * times >= intRange.min)
        {
            CurrentValue -= Increment * times;
        }
        else
        {
            CurrentValue = intRange.min;
        }

        AdjustButtonsActiveState();
    }

    internal void Load(int DefaultValue)
    {
        var Int = BetterDataManager.LoadSetting(Id, DefaultValue);

        if (Int > intRange.max || Int < intRange.min)
        {
            Int = DefaultValue;
            BetterDataManager.SaveSetting(Id, DefaultValue);
        }

        CurrentValue = Int;
    }

    internal override float GetFloat() => GetInt();

    internal override int GetInt()
    {
        if (BetterDataManager.CanLoadSetting<int>(Id))
        {
            return BetterDataManager.LoadSetting<int>(Id);
        }
        else
        {
            return CurrentValue;
        }
    }

    internal override void SetData(OptionBehaviour optionBehaviour)
    {
        optionBehaviour.data = new BaseGameSetting
        {
            Title = StringNames.None,
            Type = OptionTypes.Int,
        };
    }

    internal override void ValueChanged(int id, OptionBehaviour optionBehaviour)
    {
        if (IsParent || IsChild)
        {
            bool Bool = ShowChildrenCondition();
            foreach (var item in ChildrenList)
            {
                item.obj.SetActive(Bool && item.SelfShowCondition());
            }
            UpdatePositions();
        }
    }
}
