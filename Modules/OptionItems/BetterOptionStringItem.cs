using UnityEngine;

namespace BetterAmongUs;

public class BetterOptionStringItem : BetterOptionItem
{
    public string[] Values = [];
    public int CurrentValue;
    private StringOption? ThisOption;

    public BetterOptionItem Create(int id, GameOptionsMenu gameOptionsMenu, string name, string[] strings, int DefaultValue = 0, BetterOptionItem Parent = null)
    {
        Id = id;
        Values = strings;
        Tab = gameOptionsMenu;
        Name = name;
        if (DefaultValue < 0) DefaultValue = 0;
        if (DefaultValue > Values.Length) DefaultValue = Values.Length;
        CurrentValue = DefaultValue;

        if (gameOptionsMenu == null)
        {
            Load(DefaultValue);
            return this;
        }

        StringOption optionBehaviour = UnityEngine.Object.Instantiate<StringOption>(gameOptionsMenu.stringOptionOrigin, Vector3.zero, Quaternion.identity, gameOptionsMenu.settingsContainer);
        optionBehaviour.transform.localPosition = new Vector3(0.952f, 2f - StaticSpacingNum * SpacingNum, -2f);
        SetUp(optionBehaviour);
        optionBehaviour.OnValueChanged = new Action<OptionBehaviour>((option) => ValueChanged(id, option));
        SpacingNum += StaticSpacingNumPlus;

        // Fix Game Crash
        foreach (RulesCategory rulesCategory in GameManager.Instance.GameSettingsList.AllCategories)
        {
            optionBehaviour.data = rulesCategory.AllGameSettings.ToArray().FirstOrDefault(item => item.Type == OptionTypes.String);
        }

        optionBehaviour.PlusBtn.OnClick.RemoveAllListeners();
        optionBehaviour.MinusBtn.OnClick.RemoveAllListeners();
        optionBehaviour.PlusBtn.OnClick.AddListener(new Action(() => Increase()));
        optionBehaviour.MinusBtn.OnClick.AddListener(new Action(() => Decrease()));

        // Set data
        TitleText = optionBehaviour.TitleText;
        Option = optionBehaviour;
        ThisOption = optionBehaviour;

        Load(DefaultValue);
        AdjustButtonsActiveState();

        BetterOptionItems.Add(this);
        return this;
    }

    private void AdjustButtonsActiveState()
    {
        if (ThisOption == null) return;

        if (CurrentValue <= Values.Length - 1 && CurrentValue >= 0)
        {
            ThisOption.ValueText.text = Values[CurrentValue];
        }

        if (CurrentValue >= Values.Length - 1)
        {
            ThisOption.PlusBtn.SetInteractable(false);
            ThisOption.MinusBtn.SetInteractable(true);
        }
        else if (CurrentValue <= 0)
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
        if (CurrentValue < Values.Length)
        {
            CurrentValue++;
            AdjustButtonsActiveState();
        }
    }

    public void Decrease()
    {
        if (CurrentValue > 0)
        {
            CurrentValue--;
            AdjustButtonsActiveState();
        }
    }

    public void Load(int DefaultValue)
    {
        if (BetterDataManager.CanLoadSetting(Id))
        {
            var Int = BetterDataManager.LoadIntSetting(Id);

            if (Int > Values.Length - 1 || Int < 0)
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

    public override int GetValue()
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
            Type = OptionTypes.String,
        };
    }

    public override void ValueChanged(int id, OptionBehaviour optionBehaviour)
    {
    }
}
