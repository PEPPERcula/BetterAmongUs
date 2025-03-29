using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Linq.Expressions;
using System.Reflection;

namespace BetterAmongUs.Items.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
internal abstract class MethodPatchAttribute : Attribute
{
    private static readonly HashSet<Assembly> _registeredAssemblies = new();
    private static readonly Dictionary<MethodInfo, MethodPatchAttribute> _methodPatchAttributes = [];
    private static readonly HarmonyMethod _prefixHarmonyMethod = new(typeof(MethodPatchAttribute), nameof(PatchPrefix));
    private static readonly HarmonyMethod _postfixHarmonyMethod = new(typeof(MethodPatchAttribute), nameof(PatchPostfix));
    private static readonly HashSet<MethodInfo> _methodsToPatch = [];

    protected MethodBase? PatchedMethod { get; private set; }
    protected Delegate? DelegateMethod { get; private set; }
    private bool runOriginal;

    public static void Register(Assembly assembly, BasePlugin plugin)
    {
        if (_registeredAssemblies.Contains(assembly)) return;
        _registeredAssemblies.Add(assembly);

        foreach (var type in assembly.GetTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (method.GetCustomAttribute<MethodPatchAttribute>(true) is MethodPatchAttribute attribute)
                {
                    if (method?.DeclaringType != null && attribute.ShouldPatch(plugin, method.DeclaringType, method, method.GetParameters()))
                    {
                        attribute.PatchedMethod = method;
                        _methodPatchAttributes[method] = attribute;
                        _methodsToPatch.Add(method);
                    }
                }
            }
        }
    }

    internal static void Finished()
    {
        foreach (var method in _methodsToPatch.OrderBy(method => method.Name))
        {
            var attribute = _methodPatchAttributes[method];

            if (method is not MethodInfo methodInfo)
                throw new InvalidOperationException("OriginalMethod is not a MethodInfo");

            var paramTypes = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
            var delegateType = methodInfo.IsStatic
                ? Expression.GetDelegateType(paramTypes.Concat(new[] { methodInfo.ReturnType }).ToArray())
                : Expression.GetDelegateType([methodInfo.DeclaringType!, .. paramTypes, .. new[] { methodInfo.ReturnType }]);

            attribute.DelegateMethod = methodInfo.CreateDelegate(delegateType);

            var patch = Main.Harmony.Patch(method, _prefixHarmonyMethod, _postfixHarmonyMethod);
        }
    }

    private static bool PatchPrefix(object __instance, MethodBase __originalMethod, object[] __args)
    {
        if (_methodPatchAttributes.TryGetValue((MethodInfo)__originalMethod, out var attribute))
        {
            if (attribute.runOriginal)
            {
                attribute.runOriginal = false;
                return true;
            }

            return attribute.RunPrefix(__instance, __args);
        }

        return true;
    }

    private static void PatchPostfix(object __instance, MethodBase __originalMethod, object[] __args)
    {
        if (_methodPatchAttributes.TryGetValue((MethodInfo)__originalMethod, out var attribute))
        {
            attribute.RunPostfix(__instance, __args);
        }
    }

    internal object? InvokeOriginal(object? instance, object[] args)
    {
        if (DelegateMethod == null) return null;

        var allArgs = PatchedMethod?.IsStatic == false
            ? new[] { instance }.Concat(args).ToArray()
            : args;

        runOriginal = true;
        return DelegateMethod.DynamicInvoke(allArgs);
    }

    internal virtual bool ShouldPatch(BasePlugin plugin, Type declaringType, MethodBase method, ParameterInfo[] args) => true;
    internal abstract bool RunPrefix(object instance, object[] args);
    internal abstract void RunPostfix(object instance, object[] args);

    internal static void Initialize()
    {
        IL2CPPChainloader.Instance.PluginLoad += (_, assembly, plugin) => Register(assembly, plugin);
        IL2CPPChainloader.Instance.Finished += Finished;
    }
}