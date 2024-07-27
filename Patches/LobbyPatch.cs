using HarmonyLib;

namespace BetterAmongUs.Patches;

public class LobbyPatch
{
    [HarmonyPatch(typeof(LobbyBehaviour))]
    internal class LobbyBehaviourPatch
    {
        // Disabled annoying music
        [HarmonyPatch(nameof(LobbyBehaviour.Update))]
        [HarmonyPostfix]
        public static void Postfix(PingTracker __instance)
        {
            if (Main.DisableLobbyTheme.Value)
                SoundManager.instance.StopSound(LobbyBehaviour.Instance.MapTheme);
        }
    }
}
