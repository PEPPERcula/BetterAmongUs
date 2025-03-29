using BepInEx.Unity.IL2CPP;
using System.Collections.Concurrent;
using System.Reflection;

internal abstract class RegisterAttribute : Attribute
{
    private readonly static ConcurrentDictionary<Type, RegisterAttribute> RegisteredAttributes = new();

    public abstract void Register(Assembly assembly, BasePlugin plugin);
    public virtual void Finished() { }

    public static void Initialize<T>() where T : RegisterAttribute
    {
        if (RegisteredAttributes.ContainsKey(typeof(T)))
            return;

        try
        {
            if (Activator.CreateInstance(typeof(T)) is not T instance)
                throw new InvalidOperationException($"Failed to create instance of {typeof(T)}");

            if (RegisteredAttributes.TryAdd(typeof(T), instance))
            {
                IL2CPPChainloader.Instance.PluginLoad += (_, assembly, plugin) => instance.Register(assembly, plugin);
                IL2CPPChainloader.Instance.Finished += instance.Finished;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize {typeof(T)}", ex);
        }
    }
}