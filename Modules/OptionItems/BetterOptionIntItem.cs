using UnityEngine;

namespace BetterAmongUs;

public class BetterOptionIntItem : BetterOptionItem
{
    private NumberOption? ThisOption;
    public int CurrentValue;
    public IntRange intRange = new(0, 180);
    public int Increment = 1;
    private string? PostFix;
    private string? PreFix;

    public override bool ShowChildrenCondition() => CurrentValue > intRange.min;
    public override bool SelfShowCondition() => ShowCondition != null ? ShowCondition() : base.SelfShowCondition();
    public Func<bool>? ShowCondition = null;

    public BetterOptionItem Create(int id, GameOptionsMenu gameOptionsMenu, string name, int[] values, int DefaultValue, string preFix = "", string postFix = "", BetterOptionItem? Parent = null, Func<bool>? selfShowCondition = null)
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

        NumberOption optionBehaviour = UnityEngine.Object.Instantiate<NumberOption>(gameOptionsMenu.numberOptionOrigin, Vector3.zero, Quaternion.identity, gameOptionsMenu.settingsContainer);
        optionBehaviour.transform.localPosition = new Vector3(0.952f, 2f, -2f);
        SetUp(optionBehaviour);
        optionBehaviour.OnValueChanged = new Action<OptionBehaviour>((option) => ValueChanged(id, option));

        // Fix Game Crash
        foreach (RulesCategory rulesCategory in GameManager.Instance.GameSettingsList.AllCategories)
        {
            optionBehaviour.data = rulesCategory.AllGameSettings.ToArray().FirstOrDefault(item => item.Type == OptionTypes.Number);
        }

        optionBehaviour.PlusBtn.OnClick.RemoveAllListeners();
        optionBehaviour.MinusBtn.OnClick.RemoveAllListeners();
        optionBehaviour.PlusBtn.OnClick.AddListener(new Action(() => Increase()));
        optionBehaviour.MinusBtn.OnClick.AddListener(new Action(() => Decrease()));
        optionBehaviour.PlusBtn.OnClick.AddListener(new Action(() => ValueChanged(id, optionBehaviour)));
        optionBehaviour.MinusBtn.OnClick.AddListener(new Action(() => ValueChanged(id, optionBehaviour)));

        optionBehaviour.LabelBackground.transform.localScale = new Vector3(1.6f, 1f);
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
            optionBehaviour.LabelBackground.GetComponent<SpriteRenderer>().color = new Color(0.85f, 0.85f, 0.85f, 1f);
            optionBehaviour.LabelBackground.transform.SetLocalZ(1f);
            ThisParent = Parent;
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

        BetterDataManager.SaveSetting(Id, CurrentValue.ToString());
    }

    public void Increase()
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

        AdjustButtonsActiveState();
    }

    public void Decrease()
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

        AdjustButtonsActiveState();
    }

    public void Load(int DefaultValue)
    {
        if (BetterDataManager.CanLoadSetting(Id))
        {
            var Int = BetterDataManager.LoadIntSetting(Id, DefaultValue);

            if (Int > intRange.max || Int < intRange.min)
            {
                Int = DefaultValue;
                BetterDataManager.SaveSetting(Id, DefaultValue.ToString());
            }

            CurrentValue = Int;
        }
        else
        {
            BetterDataManager.SaveSetting(Id, DefaultValue.ToString());
        }
    }

    public override float GetFloat()
    {
        if (BetterDataManager.CanLoadSetting(Id))
        {
            return BetterDataManager.LoadIntSetting(Id);
        }
        else
        {
            return CurrentValue;
        }
    }

    public override int GetInt()
    {
        if (BetterDataManager.CanLoadSetting(Id))
        {
            return BetterDataManager.LoadIntSetting(Id);
        }
        else
        {
            return CurrentValue;
        }
    }

    public override void SetData(OptionBehaviour optionBehaviour)
    {
        optionBehaviour.data = new BaseGameSetting
        {
            Title = StringNames.None,
            Type = OptionTypes.Int,
        };
    }

    public override void ValueChanged(int id, OptionBehaviour optionBehaviour)
    {
        if (IsParent)
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
