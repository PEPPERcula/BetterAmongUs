using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using BetterAmongUs.Mono;
using BetterAmongUs.Patches.Gameplay.UI.Chat;
using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches.Gameplay.UI;

[HarmonyPatch(typeof(MeetingHud))]
class MeetingHudPatch
{
    [HarmonyPatch(nameof(MeetingHud.Start))]
    [HarmonyPostfix]
    internal static void Start_Postfix(MeetingHud __instance)
    {
        foreach (var pva in __instance.playerStates)
        {
            var target = Utils.PlayerFromPlayerId(pva.TargetPlayerId);
            pva.gameObject.AddComponent<MeetingInfoDisplay>().Init(target, pva);
        }

        Logger.LogHeader("Meeting Has Started");
    }

    internal static float timeOpen = 0f;

    // Set player meeting info
    [HarmonyPatch(nameof(MeetingHud.Update))]
    [HarmonyPostfix]
    internal static void Update_Postfix(MeetingHud __instance)
    {
        timeOpen += Time.deltaTime;
    }

    [HarmonyPatch(nameof(MeetingHud.Close))]
    [HarmonyPostfix]
    internal static void Close_Postfix()
    {
        timeOpen = 0f;
        Logger.LogHeader("Meeting Has Ended");

        if (BAUPlugin.ChatInGameplay.Value && !GameState.IsFreePlay && PlayerControl.LocalPlayer.IsAlive())
        {
            ChatPatch.ClearPlayerChats();
        }
    }
}