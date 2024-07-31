
using HarmonyLib;

// Enable modded protocol
namespace BetterAmongUs.Patches;

[HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
class ServerUpdatePatch
{

    private static int VanillaVersionServer = 0;
    private static int ModdedVersionServer = 0;
    public static bool IsModdedProtocol => ModdedVersionServer == VanillaVersionServer + 25;

    static void Postfix(ref int __result)
    {
        if (GameStates.IsLocalGame)
        {
            // Logger.Log($"IsLocalGame: {__result}", "VersionServer");
        }
        if (GameStates.IsOnlineGame)
        {
            VanillaVersionServer = __result;
            if (Main.ModdedProtocol.Value || !GameStates.IsVanillaServer)
            {
                __result += 25;
            }
            ModdedVersionServer = __result;
            // Logger.Log($"IsOnlineGame: {__result}", "VersionServer");
        }
    }
}
[HarmonyPatch(typeof(Constants), nameof(Constants.IsVersionModded))]
public static class IsVersionModdedPatch
{
    public static bool Prefix(ref bool __result)
    {
        __result = ServerUpdatePatch.IsModdedProtocol;
        return false;
    }
}