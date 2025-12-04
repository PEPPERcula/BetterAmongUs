using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Enums;
using BetterAmongUs.Managers;
using BetterAmongUs.Modules;
using BetterAmongUs.Mono;
using BetterAmongUs.Patches.Gameplay.UI.Chat;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Patches.Gameplay.UI;

[HarmonyPatch(typeof(MeetingHud))]
internal static class MeetingHudPatch
{
    [HarmonyPatch(nameof(MeetingHud.Start))]
    [HarmonyPostfix]
    private static void Start_Postfix(MeetingHud __instance)
    {
        foreach (var pva in __instance.playerStates)
        {
            var target = Utils.PlayerFromPlayerId(pva.TargetPlayerId);
            pva.gameObject.AddComponent<MeetingInfoDisplay>().Init(target, pva);
        }

        if (!GameState.IsOnlineGame) return;

        // Add host icon to meeting hud
        __instance.ProceedButton.gameObject.transform.localPosition = new(-2.5f, 2.2f, 0);
        __instance.ProceedButton.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        __instance.ProceedButton.GetComponent<PassiveButton>().enabled = false;
        __instance.HostIcon.enabled = true;
        __instance.HostIcon.gameObject.SetActive(true);
        __instance.ProceedButton.gameObject.SetActive(true);
        MeetingHud.Instance.ProceedButton.DestroyTextTranslators();
        UpdateHostIcon();

        Logger.LogHeader("Meeting Has Started");
    }

    internal static void UpdateHostIcon()
    {
        if (MeetingHud.Instance == null) return;

        PlayerMaterial.SetColors(GameData.Instance.GetHost().Color, MeetingHud.Instance.HostIcon);
        MeetingHud.Instance.ProceedButton.gameObject.GetComponentInChildren<TextMeshPro>().text = string.Format(Translator.GetString("HostInMeeting"), GameData.Instance.GetHost().BetterData().RealName);
    }

    internal static float timeOpen = 0f;

    // Set player meeting info
    [HarmonyPatch(nameof(MeetingHud.Update))]
    [HarmonyPostfix]
    private static void Update_Postfix(MeetingHud __instance)
    {
        timeOpen += Time.deltaTime;
    }

    [HarmonyPatch(nameof(MeetingHud.Close))]
    [HarmonyPostfix]
    private static void Close_Postfix()
    {
        timeOpen = 0f;
        Logger.LogHeader("Meeting Has Ended");

        if (BAUPlugin.ChatInGameplay.Value && !GameState.IsFreePlay && PlayerControl.LocalPlayer.IsAlive())
        {
            ChatPatch.ClearPlayerChats();
        }
    }

    [HarmonyPatch(nameof(MeetingHud.SetMasksEnabled))]
    [HarmonyPostfix]
    private static void SetMasksEnabled_Postfix(MeetingHud __instance)
    {
        HostManager.SyncNames(NameSyncType.Reset);
    }
}