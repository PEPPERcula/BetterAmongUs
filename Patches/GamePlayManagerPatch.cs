using HarmonyLib;
using Hazel;

namespace BetterAmongUs.Patches;

class GamePlayManager
{
    public static string ModdedProtocolWarning = $"<b><color=#ff1100><size=125%><align=\"center\">Warning</size>\n" +
    "Modded Protocol Is Enabled!</align></color></b>\n<size=120%> </size>\n"
        + "While <b><color=#4f92ff>Modded Protcol</b></color> is enabled, the built-in server-sided anti cheat is disabled, be sure to have <b><color=#4f92ff>Anti Cheat</b></color> enabled in settings.\n"
        + "Also while enabled your lobby will not be viewable to the public and only other <b><color=#4f92ff>Modded Protocol</b></color> players can find your lobby, normal players are still able to join off codes!";

    [HarmonyPatch(typeof(LobbyBehaviour))]
    public class LobbyBehaviourPatch
    {
        [HarmonyPatch(nameof(LobbyBehaviour.OnDestroy))]
        [HarmonyPrefix]
        private static void OnDestroy_Prefix(/*LobbyBehaviour __instance*/)
        {
            if (GameStates.IsInGame)
            {
                AntiCheat.PauseAntiCheat();
            }
        }
        [HarmonyPatch(nameof(LobbyBehaviour.Start))]
        [HarmonyPostfix]
        private static void Start_Postfix(/*LobbyBehaviour __instance*/)
        {
            _ = new LateTask(() =>
            {
                if (GameStates.IsInGame)
                {
                    RPC.SyncAllNames(force: true);
                }

                if (GameStates.IsModdedProtocol)
                {
                    Utils.AddChatPrivate(ModdedProtocolWarning, overrideName: " ");
                }
            }, 1.5f, "LobbyBehaviourPatch SyncAllNames");
        }

        // Disabled annoying music
        [HarmonyPatch(nameof(LobbyBehaviour.Update))]
        [HarmonyPostfix]
        public static void Update_Postfix(/*LobbyBehaviour __instance*/)
        {
            if (Main.DisableLobbyTheme.Value)
                SoundManager.instance.StopSound(LobbyBehaviour.Instance.MapTheme);
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
