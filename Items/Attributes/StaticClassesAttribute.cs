using BetterAmongUs.Commands;
using BetterAmongUs.Modules.AntiCheat;
using System.Reflection;

namespace BetterAmongUs.Items.Attributes;

public abstract class InstanceAttribute : Attribute
{
    public static void RegisterAll()
    {
        var assembly = Assembly.GetExecutingAssembly();

        var types = assembly
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(InstanceAttribute)) && !t.IsAbstract && t.IsSealed)
            .ToArray();

        foreach (var type in types)
        {
            if (Activator.CreateInstance(type) is InstanceAttribute attribute)
            {
                attribute.RegisterInstances();
            }
        }
    }

    protected abstract void RegisterInstances();
}

[AttributeUsage(AttributeTargets.Class)]
public abstract class StaticInstanceAttribute<T> : InstanceAttribute where T : class
{
    private static readonly List<T> _instances = [];
    public static IReadOnlyList<T> Instances => _instances.AsReadOnly();
    public static J? GetClassInstance<J>() where J : class => _instances.FirstOrDefault(instance => instance.GetType() == typeof(J)) as J;

    protected override void RegisterInstances()
    {
        var assembly = Assembly.GetExecutingAssembly();

        var attributedTypes = assembly.GetTypes()
            .Where(t => t.GetCustomAttributes(GetType(), false).Any());

        foreach (var type in attributedTypes)
        {
            if (typeof(T).IsAssignableFrom(type))
            {
                var instance = Activator.CreateInstance(type) as T;
                if (instance != null)
                {
                    _instances.Add(instance);
                }
            }
        }
    }
}

// Class instances
public sealed class RegisterCommandAttribute : StaticInstanceAttribute<BaseCommand>
{
}

public sealed class RegisterRPCHandlerAttribute : StaticInstanceAttribute<RPCHandler>
{
}