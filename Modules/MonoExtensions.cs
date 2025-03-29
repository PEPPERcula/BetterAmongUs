using Il2CppInterop.Runtime;
using UnityEngine;

namespace BetterAmongUs.Modules;

internal abstract class MonoExtension : MonoBehaviour
{
    private static readonly Dictionary<Type, List<ExtensionPair>> _extensionsByBaseType = new();
    private MonoBehaviour? _baseMono;

    protected MonoBehaviour? BaseMono
    {
        get => _baseMono;
        private set => _baseMono = value;
    }

    private struct ExtensionPair
    {
        public MonoBehaviour? Base;
        public MonoExtension? Extension;
    }

    public static T? Get<T>(MonoBehaviour mono) where T : MonoExtension
    {
        if (_extensionsByBaseType.TryGetValue(mono.GetType(), out var extensions))
        {
            foreach (var pair in extensions)
            {
                if (pair.Base == mono && pair.Extension is T result)
                {
                    return result;
                }
            }
        }

        return null;
    }

    protected bool RegisterExtension<T>() where T : MonoBehaviour
    {
        var baseComponent = GetComponent(Il2CppType.From(typeof(T))) as T;
        if (baseComponent == null)
        {
            Destroy(this);
            return false;
        }

        var existingExtensions = GetComponents(Il2CppType.From(GetType()));
        if (existingExtensions.Length > 1)
        {
            Destroy(this);
            return false;
        }

        var baseType = typeof(T);
        if (!_extensionsByBaseType.TryGetValue(baseType, out var extensions))
        {
            extensions = new List<ExtensionPair>();
            _extensionsByBaseType[baseType] = extensions;
        }

        extensions.Add(new ExtensionPair { Base = baseComponent, Extension = this });
        BaseMono = baseComponent;

        return true;
    }

    protected void UnregisterExtension<T>() where T : MonoBehaviour
    {
        if (BaseMono == null) return;

        if (_extensionsByBaseType.TryGetValue(typeof(T), out var extensions))
        {
            for (int i = extensions.Count - 1; i >= 0; i--)
            {
                if (extensions[i].Extension == this)
                {
                    extensions.RemoveAt(i);
                    break;
                }
            }

            if (extensions.Count == 0)
            {
                _extensionsByBaseType.Remove(typeof(T));
            }
        }

        BaseMono = null;
    }

    public static void CleanAll()
    {
        foreach (var kvp in _extensionsByBaseType.ToArray())
        {
            var extensions = kvp.Value;
            for (int i = extensions.Count - 1; i >= 0; i--)
            {
                if (extensions[i].Base == null || extensions[i].Extension == null)
                {
                    extensions.RemoveAt(i);
                }
            }

            if (extensions.Count == 0)
            {
                _extensionsByBaseType.Remove(kvp.Key);
            }
        }
    }
}