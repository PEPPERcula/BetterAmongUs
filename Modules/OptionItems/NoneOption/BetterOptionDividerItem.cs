using UnityEngine;

namespace BetterAmongUs;

public class BetterOptionDividerItem : BetterOptionItem
{
    public BetterOptionItem Create(GameOptionsMenu gameOptionsMenu)
    {
        if (gameOptionsMenu == null)
        {
            return this;
        }

        SpacingNum += 0.15f;

        Tab = gameOptionsMenu;
        Name = "Divider";
        CategoryHeaderMasked categoryHeaderMasked = UnityEngine.Object.Instantiate<CategoryHeaderMasked>(gameOptionsMenu.categoryHeaderOrigin, Vector3.zero, Quaternion.identity, gameOptionsMenu.settingsContainer);
        categoryHeaderMasked.transform.localScale = Vector3.one * 0.63f;
        categoryHeaderMasked.transform.localPosition = new Vector3(-0.903f, 2f - StaticSpacingNum * SpacingNum, -2f);
        UnityEngine.Object.Destroy(categoryHeaderMasked.Background.gameObject);
        UnityEngine.Object.Destroy(categoryHeaderMasked.Title.gameObject);
        if (categoryHeaderMasked.Divider != null)
        {
            categoryHeaderMasked.Divider.material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
        }

        SpacingNum += 0.45f;

        return this;
    }
}
