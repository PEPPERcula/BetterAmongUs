using BetterAmongUs.Helpers;
using UnityEngine;

namespace BetterAmongUs.Items.OptionItems.NoneOption;

internal sealed class OptionDividerItem : OptionItem
{
    internal override bool CanLoad => false;
    internal override bool IsOption => false;
    internal (float top, float bottom) Distance { get; set; }

    internal static OptionDividerItem Create(OptionTab tab, float topDistance = 0.26f, float bottomDistance = 0.50f)
    {
        var Item = new OptionDividerItem
        {
            Tab = tab,
            Distance = (topDistance, bottomDistance)
        };
        Item.CreateBehavior();

        return Item;
    }

    protected void CreateBehavior()
    {
        if (!GameSettingMenu.Instance) return;
        AllTBROptionsTemp.Add(this);
        CategoryHeaderMasked categoryHeaderMasked = UnityEngine.Object.Instantiate(Tab.AUTab.categoryHeaderOrigin, Tab.AUTab.settingsContainer);
        Obj = categoryHeaderMasked.gameObject;
        categoryHeaderMasked.transform.localScale = Vector3.one * 0.63f;
        categoryHeaderMasked.Background.gameObject.DestroyObj();
        categoryHeaderMasked.Title.DestroyObj();
        if (categoryHeaderMasked.Divider != null)
        {
            categoryHeaderMasked.Divider.material.SetInt(PlayerMaterial.MaskLayer, MaskLayer);
        }
        Tab.Children.Add(this);
    }

    internal sealed override string ValueAsString()
    {
        throw new NotImplementedException();
    }
}
