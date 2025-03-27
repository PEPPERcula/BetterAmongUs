using HarmonyLib;
using System.Reflection;

namespace BetterAmongUs.Items.Attributes;

[AttributeUsage(AttributeTargets.Method)]
internal abstract class MethodPatchAttribute : Attribute
{
    private static readonly Dictionary<MethodInfo, MethodPatchAttribute> _methodPatchAttributes = [];
    private static readonly HarmonyMethod _prefixHarmonyMethod = new(typeof(MethodPatchAttribute), nameof(PatchPrefix));
    private static readonly HarmonyMethod _postfixHarmonyMethod = new(typeof(MethodPatchAttribute), nameof(PatchPostfix));
    private static bool _hasPatched = false;
    private static readonly HashSet<MethodInfo> _methodsToPatch = [];

    public static void PatchAllMethods(Harmony harmony)
    {
        if (_hasPatched) return;
        _hasPatched = true;

        if (_methodsToPatch.Count == 0)
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (method.GetCustomAttribute<MethodPatchAttribute>(true) is MethodPatchAttribute attribute)
                    {
                        if (attribute.ShouldPatch(method.DeclaringType, method, method.GetParameters()))
                        {
                            _methodPatchAttributes[method] = attribute;
                            _methodsToPatch.Add(method);
                        }
                    }
                }
            }
        }

        foreach (var method in _methodsToPatch.OrderBy(method => method.Name))
        {
            harmony.Patch(method, _prefixHarmonyMethod, _postfixHarmonyMethod);
        }
    }

    private static bool PatchPrefix(object __instance, MethodBase __originalMethod, object[] __args)
    {
        if (_methodPatchAttributes.TryGetValue((MethodInfo)__originalMethod, out var attribute))
        {
            if (!attribute.RunPrefix(__instance, __originalMethod, __args))
            {
                return false;
            }
        }

        return true;
    }

    private static void PatchPostfix(object __instance, MethodBase __originalMethod, object[] __args)
    {
        if (_methodPatchAttributes.TryGetValue((MethodInfo)__originalMethod, out var attribute))
        {
            attribute.RunPostfix(__instance, __originalMethod, __args);
        }
    }

    public virtual bool ShouldPatch(Type declaringType, MethodBase method, ParameterInfo[] args) => true;

    public abstract bool RunPrefix(object instance, MethodBase method, object[] args);

    public abstract void RunPostfix(object instance, MethodBase method, object[] args);
}