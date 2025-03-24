using BetterAmongUs.Modules;
using BetterAmongUs.Patches;
using HarmonyLib;

namespace BetterAmongUs.Managers;

[HarmonyPatch]
internal static class PrivateOnlyLobbyManager
{
    internal static void SyncNames()
    {
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
