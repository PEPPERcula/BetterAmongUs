using HarmonyLib;

namespace BetterAmongUs.Patches;

[HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
class ServerUpdatePatch
{
    static void Postfix(ref int __result)
    {
        var region = ServerManager.Instance.CurrentRegion.Name;

        if (GameStates.IsLocalGame)
        {
        }
        if (GameStates.IsOnlineGame)
        {
            if (region.Contains("MNA") || region.Contains("MEU") || region.Contains("MAS"))
            {
                __result += 25;
            }
        }
    }
}
[HarmonyPatch(typeof(Constants), nameof(Constants.IsVersionModded))]
public static class IsVersionModdedPatch
{
    public static bool Prefix(ref bool __result)
    {
        var region = ServerManager.Instance.CurrentRegion.Name;

        if (region.Contains("MNA") || region.Contains("MEU") || region.Contains("MAS"))
        {
            __result = true;
            return false;
        }
        return true;
    }
}