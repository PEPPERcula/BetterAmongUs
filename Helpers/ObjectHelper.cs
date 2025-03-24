using UnityEngine;

namespace BetterAmongUs.Helpers;

internal static class ObjectHelper
{
    internal static GameObject? FindObjectByName(string objectName)
    {
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>(true);

        foreach (GameObject obj in allObjects)
        {
            if (obj.name == objectName)
            {
                return obj;
            }
        }

        return null;
    }
    internal static void DestroyObj(this GameObject obj)
    {
        if (obj != null)
        {
            UnityEngine.Object.Destroy(obj);
        }
    }
    internal static void DestroyObj(this UnityEngine.Object obj) => obj.DestroyObj();
    internal static void DestroyCom(this Component com) => com.DestroyObj();
    internal static void DestroyObj(this MonoBehaviour mono) => mono?.gameObject?.DestroyObj();
    internal static void DestroyMono(this MonoBehaviour mono) => UnityEngine.Object.Destroy(mono);

    internal static void DestroyTextTranslator(this GameObject obj)
    {
        var translator = obj.GetComponent<TextTranslatorTMP>();
        if (translator != null)
        {
            UnityEngine.Object.Destroy(translator);
        }
    }
    internal static void DestroyTextTranslator(this MonoBehaviour mono) => mono.gameObject.DestroyTextTranslator();
}
