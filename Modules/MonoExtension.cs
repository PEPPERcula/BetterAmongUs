using Il2CppInterop.Runtime;
using System.Reflection;
using UnityEngine;

namespace BetterAmongUs.Modules;

[AttributeUsage(AttributeTargets.Class)]
internal class MonoExtensionAttribute : Attribute
{
    internal Type MonoType { get; }
    internal Il2CppSystem.Type MonoIl2CppType { get; }

    internal MonoExtensionAttribute(Type monoType)
    {
        MonoType = monoType;
        MonoIl2CppType = Il2CppType.From(monoType);
    }
}

internal interface IMonoExtension
{
    MonoBehaviour? BaseMono { get; set; }
}

internal static class MonoExtensionManager
{
    private static readonly Dictionary<Type, List<ExtensionPair>> _extensionsByBaseType = [];

    internal static T? Get<T>(MonoBehaviour mono) where T : class, IMonoExtension
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

    internal static void CleanAll()
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

    internal static bool RegisterExtension(IMonoExtension extension)
    {
        var monoAttribute = extension.GetType().GetCustomAttribute<MonoExtensionAttribute>();

        if (monoAttribute == null || !monoAttribute.MonoType.IsAssignableTo(typeof(MonoBehaviour)))
        {
            if (extension is MonoBehaviour mono)
            {
                UnityEngine.Object.Destroy(mono);
            }
            return false;
        }

        var monoBehaviour = extension as MonoBehaviour;
        if (monoBehaviour == null) return false;

        var baseComponent = monoBehaviour.GetComponent(monoAttribute.MonoIl2CppType) as MonoBehaviour;
        if (baseComponent == null)
        {
            UnityEngine.Object.Destroy(monoBehaviour);
            return false;
        }

        var existingExtensions = monoBehaviour.GetComponents(Il2CppType.From(extension.GetType()));
        if (existingExtensions.Length > 1)
        {
            UnityEngine.Object.Destroy(monoBehaviour);
            return false;
        }

        if (!_extensionsByBaseType.TryGetValue(monoAttribute.MonoType, out var extensions))
        {
            extensions = [];
            _extensionsByBaseType[monoAttribute.MonoType] = extensions;
        }

        extensions.Add(new ExtensionPair { Base = baseComponent, Extension = extension });
        extension.BaseMono = baseComponent;

        return true;
    }

    internal static void UnregisterExtension(IMonoExtension extension)
    {
        if (extension.BaseMono == null) return;

        if (_extensionsByBaseType.TryGetValue(extension.BaseMono.GetType(), out var extensions))
        {
            for (int i = extensions.Count - 1; i >= 0; i--)
            {
                if (extensions[i].Extension == extension)
                {
                    extensions.RemoveAt(i);
                    break;
                }
            }

            if (extensions.Count == 0)
            {
                _extensionsByBaseType.Remove(extension.BaseMono.GetType());
            }
        }
    }

    private struct ExtensionPair
    {
        public MonoBehaviour? Base;
        public IMonoExtension? Extension;
    }
}