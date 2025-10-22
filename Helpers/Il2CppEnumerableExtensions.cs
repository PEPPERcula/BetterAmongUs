namespace BetterAmongUs.Helpers;

internal static class Il2CppEnumerableExtensions
{
    // For Il2CppSystem.Collections.Generic.IEnumerable<T>
    internal static void ForEachIl2Cpp<T>(this Il2CppSystem.Collections.Generic.IEnumerable<T> source, Action<T> action)
    {
        if (source == null || action == null) return;

        var list = new Il2CppSystem.Collections.Generic.List<T>(source);
        for (int i = 0; i < list.Count; i++)
        {
            action(list[i]);
        }
    }

    internal static T? FirstOrDefaultIl2Cpp<T>(this Il2CppSystem.Collections.Generic.IEnumerable<T> source, Func<T, bool> predicate)
    {
        if (source == null || predicate == null) return default;

        var list = new Il2CppSystem.Collections.Generic.List<T>(source);
        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];
            if (item != null && predicate(item))
                return item;
        }

        return default;
    }

    // For Il2CppSystem.Collections.Generic.List<T> - MORE EFFICIENT!
    internal static void ForEachIl2Cpp<T>(this Il2CppSystem.Collections.Generic.List<T> source, Action<T> action)
    {
        if (source == null || action == null) return;

        // Direct iteration - no conversion needed!
        for (int i = 0; i < source.Count; i++)
        {
            action(source[i]);
        }
    }

    internal static T? FirstOrDefaultIl2Cpp<T>(this Il2CppSystem.Collections.Generic.List<T> source, Func<T, bool> predicate)
    {
        if (source == null || predicate == null) return default;

        // Direct iteration - no conversion needed!
        for (int i = 0; i < source.Count; i++)
        {
            var item = source[i];
            if (item != null && predicate(item))
                return item;
        }

        return default;
    }

    public static bool AnyIl2Cpp<T>(this Il2CppSystem.Collections.Generic.List<T> list, Func<T, bool> predicate)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (predicate(list[i])) return true;
        }
        return false;
    }
}
