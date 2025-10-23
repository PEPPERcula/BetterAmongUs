using Il2CppInterop.Runtime.InteropTypes;

namespace BetterAmongUs.Helpers;

internal static class Il2CppExtensions
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

    internal static int CountIl2Cpp<T>(this Il2CppSystem.Collections.Generic.List<T> source, Func<T, bool>? predicate = null)
    {
        if (source == null) return 0;

        int count = 0;
        for (int i = 0; i < source.Count; i++)
        {
            if (predicate == null || predicate(source[i]))
                count++;
        }
        return count;
    }

    internal static List<T> WhereIl2Cpp<T>(this Il2CppSystem.Collections.Generic.List<T> source, Func<T, bool> predicate)
    {
        if (source == null || predicate == null) return [];

        var result = new List<T>();
        for (int i = 0; i < source.Count; i++)
        {
            var item = source[i];
            if (predicate(item))
                result.Add(item);
        }
        return result;
    }

    internal static bool AllIl2Cpp<T>(this Il2CppSystem.Collections.Generic.List<T> source, Func<T, bool> predicate)
    {
        if (source == null || predicate == null) return false;

        for (int i = 0; i < source.Count; i++)
        {
            if (!predicate(source[i]))
                return false;
        }
        return true;
    }

    internal static bool ContainsIl2Cpp<T>(this Il2CppSystem.Collections.Generic.List<T> source, T value) where T : Il2CppObjectBase
    {
        if (source == null) return false;

        for (int i = 0; i < source.Count; i++)
        {
            if (source[i]?.Equals(value) == true)
                return true;
        }
        return false;
    }

    internal static List<TResult> SelectIl2Cpp<T, TResult>(this Il2CppSystem.Collections.Generic.List<T> source, Func<T, TResult> selector)
    {
        if (source == null || selector == null) return [];

        var result = new List<TResult>();
        for (int i = 0; i < source.Count; i++)
        {
            result.Add(selector(source[i]));
        }
        return result;
    }
}
