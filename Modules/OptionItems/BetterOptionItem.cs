using TMPro;
using UnityEngine;

namespace BetterAmongUs;

public class BetterOptionItem
{
    public float StaticSpacingNum => 0.45f;
    public static List<BetterOptionItem> BetterOptionItems = [];
    public static float SpacingNum = 0;

    public int maskLayer = 20;
    public int Id = 0;
    public GameOptionsMenu? Tab;
    public OptionBehaviour? Option;
    public string? Name = "None";
    public TextMeshPro? TitleText;

    public void SetUp(OptionBehaviour optionBehaviour)
    {
        SetData(optionBehaviour);
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

    public virtual bool GetBool()
    {
        throw new NotImplementedException();
    }

    public virtual float GetFloat()
    {
        throw new NotImplementedException();
    }

    public virtual int GetInt()
    {
        throw new NotImplementedException();
    }

    public virtual int GetValue()
    {
        throw new NotImplementedException();
    }

    public virtual void SetData(OptionBehaviour optionBehaviour)
    {
        throw new NotImplementedException();
    }

    public virtual void ValueChanged(int id, OptionBehaviour optionBehaviour)
    {
        throw new NotImplementedException();
    }
}
