using BetterAmongUs.Helpers;
using BetterAmongUs.Patches;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Items.OptionItems;

internal class BetterOptionItem
{
    internal float StaticSpacingNum => 1f;
    internal float StaticSpacingNumPlus => 0.45f;
    internal static Dictionary<int, int> TempPlayerOptionData = [];
    internal static int TempPlayerOptionDataNum = 0;
    internal static List<BetterOptionItem> BetterOptionItems = [];
    internal static float SpacingNum = 0;

    internal int maskLayer = 20;
    internal int Id = 0;
    internal GameOptionsMenu? Tab;
    internal OptionBehaviour? Option;
    internal string? Name = "None";
    internal TextMeshPro? TitleText;
    internal GameObject? obj;

    internal BetterOptionItem? ThisParent;
    internal bool IsChild = false;
    internal bool IsParent => ChildrenList.Count > 0;
    internal List<BetterOptionItem> ChildrenList = [];
    internal virtual bool ShowChildrenCondition() => false;
    internal virtual bool SelfShowCondition() => true;

    internal static void UpdatePositions()
    {
        SpacingNum = 0f;

        foreach (var item in BetterOptionItems)
        {
            try
            {
                item.obj.transform.SetLocalY(2f);

                // Need to fix name not setting
                if (item.ThisParent != null)
                {
                    _ = new LateTask(() =>
                    {
                        item.obj.SetActive(item.ThisParent.ShowChildrenCondition() && item.SelfShowCondition() && item.ThisParent.Option.gameObject.active);
                    }, 0.005f, shouldLog: false);
                    if (!(item.ThisParent.ShowChildrenCondition() && item.SelfShowCondition() && item.ThisParent.Option.gameObject.active))
                        continue;
                }

                if (item is BetterOptionPlayerItem player)
                {
                    player.Load();
                }

                SpacingNum += item switch
                {
                    BetterOptionHeaderItem => 0.1f,
                    BetterOptionDividerItem => 0.15f,
                    _ => 0f,
                };

                item.obj.transform.SetLocalY(2f - 1f * SpacingNum);

                SpacingNum += item switch
                {
                    BetterOptionHeaderItem => 0.75f,
                    BetterOptionTitleItem => 0.50f,
                    _ => 0.45f,
                };
            }
            catch { }
        }

        GameSettingMenuPatch.BetterSettingsTab.scrollBar.SetYBoundsMax(1.5f * SpacingNum / 2);
        GameSettingMenuPatch.BetterSettingsTab.scrollBar.ScrollRelative(new(0f, 0f));
    }

    internal void SetUp(OptionBehaviour optionBehaviour)
    {
        SpriteRenderer[] componentsInChildren = optionBehaviour.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            componentsInChildren[i].material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
        }
        foreach (TextMeshPro textMeshPro in optionBehaviour.GetComponentsInChildren<TextMeshPro>(true))
        {
            textMeshPro.fontMaterial.SetFloat("_StencilComp", 3f);
            textMeshPro.fontMaterial.SetFloat("_Stencil", maskLayer);
        }
    }

    internal virtual bool GetBool()
    {
        throw new NotImplementedException();
    }

    internal virtual float GetFloat()
    {
        throw new NotImplementedException();
    }

    internal virtual int GetInt()
    {
        throw new NotImplementedException();
    }

    internal virtual int GetValue()
    {
        throw new NotImplementedException();
    }

    internal virtual void SetData(OptionBehaviour optionBehaviour)
    {
        throw new NotImplementedException();
    }

    internal virtual void ValueChanged(int id, OptionBehaviour optionBehaviour)
    {
        throw new NotImplementedException();
    }
}
