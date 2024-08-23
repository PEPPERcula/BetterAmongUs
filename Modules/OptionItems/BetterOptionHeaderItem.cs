using UnityEngine;

namespace BetterAmongUs;

public class BetterOptionHeaderItem : BetterOptionItem
{
    public BetterOptionItem Create(GameOptionsMenu gameOptionsMenu, string name)
    {
        if (gameOptionsMenu == null)
        {
            return this;
        }

        Tab = gameOptionsMenu;
        Name = name;
        CategoryHeaderMasked categoryHeaderMasked = UnityEngine.Object.Instantiate<CategoryHeaderMasked>(gameOptionsMenu.categoryHeaderOrigin, Vector3.zero, Quaternion.identity, gameOptionsMenu.settingsContainer);
        categoryHeaderMasked.transform.localScale = Vector3.one * 0.63f;
        categoryHeaderMasked.transform.localPosition = new Vector3(-0.903f, 2f - 0.45f * SpacingNum, -2f);
        categoryHeaderMasked.Title.text = name;
        categoryHeaderMasked.Background.material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
        if (categoryHeaderMasked.Divider != null)
        {
            categoryHeaderMasked.Divider.material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
        }
        categoryHeaderMasked.Title.fontMaterial.SetFloat("_StencilComp", 3f);
        categoryHeaderMasked.Title.fontMaterial.SetFloat("_Stencil", maskLayer);

        SpacingNum += 1.6f;

        return this;
    }
}
