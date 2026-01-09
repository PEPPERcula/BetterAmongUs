using BetterAmongUs.Helpers;
using HarmonyLib;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace BetterAmongUs.Patches.Client;

[HarmonyPatch]
internal static class HttpPatch
{
    public static string GetHeader()
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.Append(ModInfo.PluginVersion);
        stringBuilder.Append(';');
        stringBuilder.Append(Enum.GetName(ModInfo.ReleaseBuildType));
        stringBuilder.Append(';');
        stringBuilder.Append(ModInfo.IsHotFix);
        stringBuilder.Append('/');
        stringBuilder.Append(ModInfo.HotfixNum);
        stringBuilder.Append('/');
        stringBuilder.Append(ModInfo.BetaNum);

        return stringBuilder.ToString();
    }

    [HarmonyPatch(typeof(UnityWebRequest), nameof(UnityWebRequest.SendWebRequest))]
    private static class SendWebRequestPatch
    {
        private static void Prefix(UnityWebRequest __instance)
        {
            var path = new Uri(__instance.url).AbsolutePath;
            if (path.Contains("/api/games"))
            {
                __instance.SetRequestHeader("BAU-Mod", GetHeader());
            }
        }

        private static void Postfix(UnityWebRequest __instance, UnityWebRequestAsyncOperation __result)
        {
            var path = new Uri(__instance.url).AbsolutePath;
            if (path.Contains("/api/games"))
            {
                __result.add_completed((Action<AsyncOperation>)(_ =>
                {
                    if (!HttpUtils.IsSuccess(__instance.responseCode)) return;

                    var responseHeader = __instance.GetResponseHeader("BAU-Mod-Processed");

                    if (responseHeader != null)
                    {
                        Logger_.Log("Connected to a supported Better Among Us matchmaking server");
                    }
                }));
            }
        }
    }
}
