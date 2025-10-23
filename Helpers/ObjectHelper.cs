using UnityEngine;

namespace BetterAmongUs.Helpers;

internal static class ObjectHelper
{
    /// <summary>
    /// Destroys a GameObject if it is not null.
    /// </summary>
    internal static void DestroyObj(this GameObject obj)
    {
        if (obj != null)
        {
            UnityEngine.Object.Destroy(obj);
        }
    }

    /// <summary>
    /// Destroys the GameObject associated with a MonoBehaviour if it is not null.
    /// </summary>
    internal static void DestroyObj(this MonoBehaviour mono) => mono?.gameObject?.DestroyObj();

    /// <summary>
    /// Destroys a MonoBehaviour component if it is not null.
    /// </summary>
    internal static void DestroyMono(this MonoBehaviour mono) => UnityEngine.Object.Destroy(mono);

    /// <summary>
    /// Destroys all TextTranslatorTMP components in the children of a GameObject.
    /// </summary>
    internal static void DestroyTextTranslators(this GameObject obj)
    {
        var translators = obj.GetComponentsInChildren<TextTranslatorTMP>();
        if (translators.Length > 0)
        {
            foreach (var item in translators)
            {
                item.DestroyMono();
            }
        }
    }

    /// <summary>
    /// Destroys all TextTranslatorTMP components in the children of a MonoBehaviour's GameObject.
    /// </summary>
    internal static void DestroyTextTranslators(this MonoBehaviour mono) => mono.gameObject.DestroyTextTranslators();

    internal static void SetSpriteColors(this GameObject go, Color color)
    {
        var sprites = go.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sprite in sprites)
        {
            sprite.color = color;
        }
    }

    internal static void SetSpriteColors(this GameObject go, Action<SpriteRenderer> setSprite)
    {
        var sprites = go.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sprite in sprites)
        {
            setSprite(sprite);
        }
    }

    internal static void SetUIColors(this GameObject go, params string[] avoidGoName)
    {
        go.SetUIColors(null, null, avoidGoName);
    }

    internal static void SetUIColors(this GameObject go, Func<SpriteRenderer, bool>? check = null, params string[] avoidGoName)
    {
        go.SetUIColors(null, check, avoidGoName);
    }

    internal static void SetUIColors(this GameObject go, Color? color = null, Func<SpriteRenderer, bool>? check = null, params string[] avoidGoName)
    {
        var sprites = go.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sprite in sprites)
        {
            if (avoidGoName.Any(name => sprite.gameObject.name == name)) continue;
            if (check == null || check(sprite))
                AddColor(sprite, color);
        }
    }

    internal static void AddColor(SpriteRenderer sprite, Color? color = null)
    {
        color ??= Color.green;
        sprite.color = (sprite.color * 0.6f) + ((Color)color * 0.5f);
    }

    internal static void SetLayers(this GameObject go, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1) return;

        Transform[] allChildren = go.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            child.gameObject.layer = layer;
        }
    }
}
