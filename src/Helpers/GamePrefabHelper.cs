using Il2CppInterop.Runtime;
using UnityEngine;

namespace BetterAmongUs.Helpers;

internal static class GamePrefabHelper
{
    internal static UnityEngine.Object? GetPrefabByName(string objectName)
    {
        UnityEngine.Object[] allObjects = Resources.FindObjectsOfTypeAll(Il2CppType.Of<UnityEngine.Object>());

        var obj = allObjects.FirstOrDefault(obj => obj.hideFlags == HideFlags.None && obj.name == objectName);
        if (obj != null)
        {
            return obj;
        }

        return null;
    }
}
