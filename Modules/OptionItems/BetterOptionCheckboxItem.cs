using UnityEngine;

namespace BetterAmongUs;

public class BetterOptionCheckboxItem : BetterOptionItem
{
    private ToggleOption? ThisOption;
    private bool? IsChecked;

    public BetterOptionItem Create(int id, GameOptionsMenu gameOptionsMenu, string name, bool Default = true, BetterOptionItem Parent = null)
    {
        Id = id;

        if (gameOptionsMenu == null)
        {
            Load(Default);
            return this;
        }

        ToggleOption optionBehaviour = UnityEngine.Object.Instantiate<ToggleOption>(gameOptionsMenu.checkboxOrigin, Vector3.zero, Quaternion.identity, gameOptionsMenu.settingsContainer);
        optionBehaviour.transform.localPosition = new Vector3(0.952f, 2f - StaticSpacingNum * SpacingNum, -2f);
        SetUp(optionBehaviour);
        optionBehaviour.OnValueChanged = new Action<OptionBehaviour>((option) => ValueChanged(id, option));
        SpacingNum += StaticSpacingNumPlus;

        // Set data
        optionBehaviour.CheckMark.GetComponent<SpriteRenderer>().enabled = Default;
        IsChecked = Default;
        Tab = gameOptionsMenu;
        Name = name;
        TitleText = optionBehaviour.TitleText;
        Option = optionBehaviour;
        ThisOption = optionBehaviour;

        Load(Default);

        BetterOptionItems.Add(this);
        return this;
    }

    public void Load(bool Default)
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
            BetterDataManager.SaveSetting(Id, Default.ToString());
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
