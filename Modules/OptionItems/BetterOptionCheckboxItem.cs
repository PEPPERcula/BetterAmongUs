using UnityEngine;

namespace BetterAmongUs;

public class BetterOptionCheckboxItem : BetterOptionItem
{
    private ToggleOption? ThisOption;
    private bool? IsChecked;

    public BetterOptionItem Create(int id, GameOptionsMenu gameOptionsMenu, string name, bool DefaultValue = true, BetterOptionItem Parent = null)
    {
        Id = id;
        Tab = gameOptionsMenu;
        Name = name;
        IsChecked = DefaultValue;

        if (gameOptionsMenu == null)
        {
            Load(DefaultValue);
            return this;
        }

        ToggleOption optionBehaviour = UnityEngine.Object.Instantiate<ToggleOption>(gameOptionsMenu.checkboxOrigin, Vector3.zero, Quaternion.identity, gameOptionsMenu.settingsContainer);
        optionBehaviour.transform.localPosition = new Vector3(0.952f, 2f - StaticSpacingNum * SpacingNum, -2f);
        SetUp(optionBehaviour);
        optionBehaviour.OnValueChanged = new Action<OptionBehaviour>((option) => ValueChanged(id, option));
        SpacingNum += StaticSpacingNumPlus;

        // Fix Game Crash
        foreach (RulesCategory rulesCategory in GameManager.Instance.GameSettingsList.AllCategories)
        {
            optionBehaviour.data = rulesCategory.AllGameSettings.ToArray().FirstOrDefault(item => item.Type == OptionTypes.Checkbox);
        }

        // Set data
        optionBehaviour.CheckMark.GetComponent<SpriteRenderer>().enabled = DefaultValue;
        TitleText = optionBehaviour.TitleText;
        Option = optionBehaviour;
        ThisOption = optionBehaviour;

        Load(DefaultValue);

        BetterOptionItems.Add(this);
        return this;
    }

    public void Load(bool DefaultValue)
    {
        if (BetterDataManager.CanLoadSetting(Id))
        {
            if (ThisOption != null)
            {
                var Bool = BetterDataManager.LoadBoolSetting(Id);
                ThisOption.CheckMark.GetComponent<SpriteRenderer>().enabled = Bool;
                IsChecked = Bool;
            }
        }
        else
        {
            BetterDataManager.SaveSetting(Id, DefaultValue.ToString());
        }
    }

    public override bool GetBool()
    {
        if (BetterDataManager.CanLoadSetting(Id))
        {
            return BetterDataManager.LoadBoolSetting(Id);
        }
        else
        {
            return IsChecked ??= false;
        }
    }

    public override void SetData(OptionBehaviour optionBehaviour)
    {
        optionBehaviour.data = new BaseGameSetting
        {
            Title = StringNames.None,
            Type = OptionTypes.Checkbox,
        };
    }

    public override void ValueChanged(int id, OptionBehaviour optionBehaviour)
    {
        IsChecked = !IsChecked;
        BetterDataManager.SaveSetting(Id, IsChecked.ToString());
    }
}
