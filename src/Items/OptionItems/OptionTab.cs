using BetterAmongUs.Helpers;
using BetterAmongUs.Items.OptionItems.NoneOption;
using BetterAmongUs.Modules;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Items.OptionItems;

internal sealed class OptionTab
{
    internal static List<OptionTab> AllTabs = [];

    internal readonly List<OptionItem> Children = [];
    internal int Id { get; private set; }
    internal string? Name => Translator.GetString(TranName);
    internal string? TranName { get; private set; }
    internal string? Description => Translator.GetString(TranDescription);
    internal string? TranDescription { get; private set; }
    internal GameOptionsMenu? AUTab { get; private set; }
    internal PassiveButton? TabButton { get; private set; }
    internal Color Color { get; private set; }

    internal static OptionTab Create(int Id, string tranName, string tranDescription, Color Color, bool doNotDestroyMapPicker = false)
    {
        if (GetTabById(Id) is OptionTab optionTab)
        {
            optionTab.Children.Clear();
            optionTab.CreateBehavior(doNotDestroyMapPicker);
            return optionTab;
        }

        var Item = new OptionTab
        {
            Id = Id,
            TranName = tranName,
            TranDescription = tranDescription,
            Color = Color
        };
        AllTabs.Add(Item);

        Item.CreateBehavior(doNotDestroyMapPicker);
        return Item;
    }

    internal static OptionTab? GetTabById(int id) => AllTabs.FirstOrDefault(tab => tab.Id == id);

    private void CreateBehavior(bool doNotDestroyMapPicker)
    {
        if (!GameSettingMenu.Instance) return;

        var SettingsButton = UnityEngine.Object.Instantiate(GameSettingMenu.Instance.GameSettingsButton, GameSettingMenu.Instance.GameSettingsButton.transform.parent);
        TabButton = SettingsButton;
        SettingsButton.DestroyTextTranslators();
        var title = SettingsButton.GetComponentInChildren<TextMeshPro>();
        title?.SetText(Name);

        SettingsButton.gameObject.SetActive(true);
        SettingsButton.name = Name;
        SettingsButton.OnClick.RemoveAllListeners();
        SettingsButton.OnMouseOver.RemoveAllListeners();

        var darkColor = Color * 0.5f;
        SettingsButton.activeSprites.GetComponent<SpriteRenderer>().color = darkColor * 0.9f;
        SettingsButton.inactiveSprites.GetComponent<SpriteRenderer>().color = darkColor * 0.8f;
        SettingsButton.selectedSprites.GetComponent<SpriteRenderer>().color = darkColor;
        SettingsButton.activeTextColor = Color * 0.9f;
        SettingsButton.inactiveTextColor = Color * 0.8f;
        SettingsButton.selectedTextColor = Color;

        SettingsButton.gameObject.GetComponent<BoxCollider2D>().size = new Vector2(2.5f, 0.6176f);

        SettingsButton.OnClick.AddListener(new Action(() =>
        {
            GameSettingMenu.Instance.ChangeTab(Id, false);
        }));

        var SettingsTab = UnityEngine.Object.Instantiate(GameSettingMenu.Instance.GameSettingsTab, GameSettingMenu.Instance.GameSettingsTab.transform.parent);
        AUTab = SettingsTab;
        SettingsTab.name = Name;
        if (!doNotDestroyMapPicker) SettingsTab.scrollBar.Inner.DestroyChildren();

        AUTab.gameObject.SetActive(false);
    }

    internal void UpdateVisuals()
    {
        ShowOptions();
    }

    private void ShowOptions()
    {
        if (AUTab == null) return;

        AUTab.gameObject.SetActive(true);
        float spacingNum = 0f;
        foreach (var opt in Children)
        {
            if (opt?.Obj == null) continue;
            if (opt?.Tab.Id != Id || opt.Hide)
            {
                opt.Obj.gameObject.SetActive(false);
                continue;
            }

            opt.Obj.gameObject.SetActive(true);

            spacingNum += opt switch
            {
                OptionHeaderItem headerItem => headerItem.Distance.top,
                OptionTitleItem titleItem => titleItem.Distance.top,
                OptionDividerItem dividerItem => dividerItem.Distance.top,
                _ => 0f,
            };

            if (opt.IsOption)
            {
                opt.Obj.transform.localPosition = new Vector3(1.4f, 2f - 1f * spacingNum, 0f);
            }
            else
            {
                opt.Obj.transform.localPosition = new Vector3(-0.6f, 2f - 1f * spacingNum, 0f);
            }

            spacingNum += opt switch
            {
                OptionHeaderItem headerItem => headerItem.Distance.bottom,
                OptionTitleItem titleItem => titleItem.Distance.bottom,
                OptionDividerItem dividerItem => dividerItem.Distance.bottom,
                _ => 0.45f,
            };

            opt.UpdateVisuals(false);
        }

        AUTab?.scrollBar?.SetYBoundsMax(spacingNum - 2.5f);
        AUTab?.scrollBar?.ScrollRelative(new(0f, 0f));
    }

    internal static void FindOptions(string name)
    {
        throw new NotImplementedException();
    }
}
