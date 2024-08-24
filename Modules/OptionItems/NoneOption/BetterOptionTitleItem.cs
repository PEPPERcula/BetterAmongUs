using Rewired;
using UnityEngine;

namespace BetterAmongUs;

public class BetterOptionTitleItem : BetterOptionItem
{
    public BetterOptionItem Create(GameOptionsMenu gameOptionsMenu, string name)
    {
        if (gameOptionsMenu == null)
        {
            return this;
        }

        ToggleOption optionBehaviour = UnityEngine.Object.Instantiate<ToggleOption>(gameOptionsMenu.checkboxOrigin, Vector3.zero, Quaternion.identity, gameOptionsMenu.settingsContainer);
        optionBehaviour.transform.localPosition = new Vector3(0.952f, 2f - StaticSpacingNum * SpacingNum, -2f);
        SetUp(optionBehaviour);
        SpacingNum += StaticSpacingNumPlus;

        UnityEngine.Object.Destroy(optionBehaviour.CheckMark.transform.parent.gameObject);
        optionBehaviour.TitleText.alignment = TMPro.TextAlignmentOptions.Center;
        optionBehaviour.TitleText.outlineColor = Color.black;
        optionBehaviour.TitleText.outlineWidth = 0.2f;
        optionBehaviour.TitleText.transform.localPosition += new Vector3(0f, 0.05f, 0f);
        optionBehaviour.LabelBackground.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        optionBehaviour.LabelBackground.transform.SetLocalZ(1f);

        // Set data
        Tab = gameOptionsMenu;
        Name = $"<b><size=150%>{name}</size></b>";
        Option = optionBehaviour;
        TitleText = optionBehaviour.TitleText;

        BetterOptionItems.Add(this);
        return this;
    }

    public override void SetData(OptionBehaviour optionBehaviour)
    {
        optionBehaviour.data = new BaseGameSetting
        {
            Title = StringNames.None,
            Type = OptionTypes.Checkbox,
        };
    }
}
