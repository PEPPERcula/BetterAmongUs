using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using HarmonyLib;

namespace BetterAmongUs.Managers;

[HarmonyPatch]
internal static class PrivateOnlyLobbyManager
{
    private static void SyncNamesAsHost()
    {
        if (!GameState.IsHost || !GameState.IsPrivateOnlyLobby || !GameState.InGame) return;

        foreach (var target in PlayerControl.AllPlayerControls)
        {
            if (target.IsHost() || target.BetterData().IsBetterUser) continue;

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (GameState.IsMeeting)
                {
                    SetNameMeeting(player, target);
                }
                else
                {
                    SetNameGameplay(player, target);
                }
            }
        }
    }

    private static void SetNameGameplay(PlayerControl player, PlayerControl target)
    {
        player.RpcSetNamePrivate("", target);
    }

    private static void SetNameMeeting(PlayerControl player, PlayerControl target)
    {
        player.RpcSetNamePrivate("", target);
    }

    [HarmonyPatch(typeof(PlayerControl))]
    [HarmonyPatch(nameof(PlayerControl.Die))]
    [HarmonyPostfix]
    internal static void PlayerControlDie_Postfix(PlayerControl __instance)
    {
        if (GameState.IsPrivateOnlyLobby && BetterGameSettings.RemovePetOnDeath.GetBool())
        {
            __instance.RpcSetPet(PetData.EmptyId);
        }
    }
}
