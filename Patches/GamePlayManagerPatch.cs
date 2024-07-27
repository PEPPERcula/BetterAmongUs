using HarmonyLib;

namespace BetterAmongUs.Patches;

class GamePlayManager
{
    [HarmonyPatch(typeof(GameStartManager))]
    public class GameStartManagerPatch
    {
        [HarmonyPatch(nameof(GameStartManager.BeginGame))]
        [HarmonyPostfix]
        public static void Postfix(/*GameStartManager __instance*/)
        {
            AntiCheat.PauseAntiCheat();
        }
    }

    [HarmonyPatch(typeof(GameManager))]
    public class GameManagerPatch
    {
        [HarmonyPatch(nameof(GameManager.EndGame))]
        [HarmonyPostfix]
        public static void Postfix(/*GameManager __instance*/)
        {
            if (GameStates.IsHost)
            {
                foreach (PlayerControl player in Main.AllPlayerControls)
                {
                    player.RpcSetName(player.Data.PlayerName);
                }
            }
        }
    }
}
