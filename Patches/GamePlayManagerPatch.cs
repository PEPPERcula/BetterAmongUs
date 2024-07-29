using HarmonyLib;
using Hazel;

namespace BetterAmongUs.Patches;

class GamePlayManager
{
    [HarmonyPatch(typeof(LobbyBehaviour))]
    public class LobbyBehaviourPatch
    {
        [HarmonyPatch(nameof(LobbyBehaviour.OnDestroy))]
        [HarmonyPrefix]
        private static void Prefix(/*LobbyBehaviour __instance*/)
        {
            if (GameStates.IsInGame)
            {
                AntiCheat.PauseAntiCheat();
            }
        }
        [HarmonyPatch(nameof(LobbyBehaviour.Start))]
        [HarmonyPostfix]
        private static void Postfix(/*LobbyBehaviour __instance*/)
        {
            _ = new LateTask(() =>
            {
                if (GameStates.IsInGame)
                {
                    RPC.SyncAllNames(force: true);
                }
            }, 1.5f, "LobbyBehaviourPatch SyncAllNames");
        }
    }

    [HarmonyPatch(typeof(GameManager))]
    public class GameManagerPatch
    {
        [HarmonyPatch(nameof(GameManager.EndGame))]
        [HarmonyPostfix]
        private static void Postfix(/*GameManager __instance*/)
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

    [HarmonyPatch(typeof(GameStartManager))]
    public class GameStartManagerPatch
    {
        [HarmonyPatch(nameof(GameStartManager.Update))]
        [HarmonyPrefix]
        private static void Prefix(GameStartManager __instance)
        {
            __instance.MinPlayers = 1;
        }
    }
}
