using BetterAmongUs.Helpers;
using UnityEngine;

namespace BetterAmongUs.Items.OptionItems;

internal class BetterOptionDividerItem : BetterOptionItem
{
    internal BetterOptionItem Create(GameOptionsMenu gameOptionsMenu)
    {
        if (gameOptionsMenu == null)
        {
            return this;
        }

        Tab = gameOptionsMenu;
        Name = "Divider";
        CategoryHeaderMasked categoryHeaderMasked = UnityEngine.Object.Instantiate(gameOptionsMenu.categoryHeaderOrigin, Vector3.zero, Quaternion.identity, gameOptionsMenu.settingsContainer);
        categoryHeaderMasked.transform.localScale = Vector3.one * 0.63f;
        categoryHeaderMasked.transform.localPosition = new Vector3(-0.903f, 2f, -2f);
        categoryHeaderMasked.Background.gameObject.DestroyObj();
        categoryHeaderMasked.Title.gameObject.DestroyObj();
        if (categoryHeaderMasked.Divider != null)
        {
            categoryHeaderMasked.Divider.material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
        }

        BetterOptionItems.Add(this);
        obj = categoryHeaderMasked.gameObject;

        return this;
    }
}
