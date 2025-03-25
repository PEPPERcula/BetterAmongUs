namespace BetterAmongUs.Helpers;

internal static class CastHelper
{
    internal static bool TryCast<T>(this object obj) => obj is T;

    internal static bool TryCast<T>(this object obj, out T? item) where T : class
    {
        if (obj != null && obj is T casted)
        {
            item = casted;
            return true;
        }
        else
        {
            item = null;
            return false;
        }
    }
}
