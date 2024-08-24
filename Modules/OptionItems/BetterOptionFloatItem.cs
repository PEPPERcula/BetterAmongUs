using UnityEngine;

namespace BetterAmongUs;

public class BetterOptionFloatItem : BetterOptionItem
{
    public float CurrentValue;
    public FloatRange floatRange = new(0f, 180f);
    public float Increment = 2.5f;
    private NumberOption? ThisOption;
    private string? PostFix;
    private string? PreFix;

    public BetterOptionItem Create(int id, GameOptionsMenu gameOptionsMenu, string name, float[] values, float Default, string preFix = "", string postFix = "", BetterOptionItem Parent = null)
    {
        Id = id;

        if (gameOptionsMenu == null)
        {
            Load(Default);
            return this;
        }

        if (values.Length is < 3 or > 3) return null;

        NumberOption optionBehaviour = UnityEngine.Object.Instantiate<NumberOption>(gameOptionsMenu.numberOptionOrigin, Vector3.zero, Quaternion.identity, gameOptionsMenu.settingsContainer);
        optionBehaviour.transform.localPosition = new Vector3(0.952f, 2f - StaticSpacingNum * SpacingNum, -2f);
        SetUp(optionBehaviour);
        optionBehaviour.OnValueChanged = new Action<OptionBehaviour>((option) => ValueChanged(id, option));
        SpacingNum += StaticSpacingNumPlus;

        // Fix Game Crash
        foreach (RulesCategory rulesCategory in GameManager.Instance.GameSettingsList.AllCategories)
        {
            optionBehaviour.data = rulesCategory.AllGameSettings.ToArray().FirstOrDefault(item => item.Type == OptionTypes.Number);
        }

        optionBehaviour.PlusBtn.OnClick.RemoveAllListeners();
        optionBehaviour.MinusBtn.OnClick.RemoveAllListeners();
        optionBehaviour.PlusBtn.OnClick.AddListener(new Action(() => Increase()));
        optionBehaviour.MinusBtn.OnClick.AddListener(new Action(() => Decrease()));

        // Set data
        Tab = gameOptionsMenu;
        Name = name;
        TitleText = optionBehaviour.TitleText;
        Option = optionBehaviour;
        PostFix = postFix;
        PreFix = preFix;
        ThisOption = optionBehaviour;

        floatRange = new(values[0], values[1]);
        Increment = values[2];

        if (Default < floatRange.min) Default = floatRange.min;
        if (Default > floatRange.max) Default = floatRange.max;
        CurrentValue = Default;

        Load(Default);
        AdjustButtonsActiveState();

        BetterOptionItems.Add(this);
        return this;
    }

    private void AdjustButtonsActiveState()
    {
        if (ThisOption == null) return;

        ThisOption.ValueText.text = PreFix + CurrentValue.ToString() + PostFix;

        if (CurrentValue + Increment > floatRange.max)
        {
            ThisOption.PlusBtn.SetInteractable(false);
            ThisOption.MinusBtn.SetInteractable(true);
        }
        else if (CurrentValue - Increment < floatRange.min)
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

        if (CurrentValue + Increment * times <= floatRange.max)
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

        if (CurrentValue - Increment * times >= floatRange.min)
        {
            CurrentValue -= Increment * times;
        }

        AdjustButtonsActiveState();
    }

    public void Load(float DefaultValue)
    {
        if (BetterDataManager.CanLoadSetting(Id))
        {
            if (ThisOption != null)
            {
                var Float = BetterDataManager.LoadFloatSetting(Id);
                CurrentValue = Float;
            }
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
            return BetterDataManager.LoadFloatSetting(Id);
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
            return (int)BetterDataManager.LoadFloatSetting(Id);
        }
        else
        {
            return (int)CurrentValue;
        }
    }

    public override void SetData(OptionBehaviour optionBehaviour)
    {
        optionBehaviour.data = new BaseGameSetting
        {
            Title = StringNames.None,
            Type = OptionTypes.Float,
        };
    }

    public override void ValueChanged(int id, OptionBehaviour optionBehaviour)
    {
    }
}
