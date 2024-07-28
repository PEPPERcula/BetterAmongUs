using HarmonyLib;
using UnityEngine;


namespace BetterAmongUs.Patches;

[HarmonyPatch(typeof(HudManager))]
public class HudManagerPatch
{
    public static string WelcomeMessage = $"<b><color=#00b530><size=125%><align=\"center\">Welcome To Better Among Us\n{Main.GetVersionText()}</size>\n" +
        "Thanks for downloading!</align></color></b>\n<size=120%> </size>\n" +
        "<color=#0dff00>BAU</color> Is a mod for improving the vanilla Among Us experience with a built-in Anti-Cheat and other futures, <color=#0dff00>BAU</color> is a client-sided mod so it can be used with other vanilla Among Us players.\n\n" +
        "<color=#0dff00>BAU</color> Also has a option in <color=#2b7500>Better Options</color> on the pause menu called <color=#4f92ff>Better Host</color>, Better Host is designed to enhance the vanilla experience for other players without having <color=#0dff00>BAU</color> installed!";

    private static bool HasBeenWelcomed = false;
    [HarmonyPatch(nameof(HudManager.Start))]
    [HarmonyPostfix]
    public static void Start_Postfix(HudManager __instance)
    {
        _ = new LateTask(() =>
        {
            try
            {
                if (BAUNotificationManager.BAUNotificationManagerObj == null)
                {
                    bool ChatState = GameObject.Find("ChatUi");
                    __instance.Chat.gameObject.SetActive(true);
                    __instance.Chat.chatNotification.gameObject.SetActive(true);
                    __instance.Chat.chatNotification.SetUp(PlayerControl.LocalPlayer, "");
                    __instance.Chat.chatNotification.timeOnScreen = 0f;
                    __instance.Chat.chatNotification.gameObject.SetActive(false);
                    __instance.Chat.gameObject.SetActive(ChatState);
                }

                if (!HasBeenWelcomed && GameStates.IsInGame && GameStates.IsLobby && !GameStates.IsFreePlay)
                {
                    BAUNotificationManager.Notify("<b><color=#00751f>Welcome To Better Among Us!</color></b>", 8f);

                    Utils.AddChatPrivate(WelcomeMessage, overrideName: " ");
                    HasBeenWelcomed = true;
                }
            }
            catch { }
        }, 1f, "HudManagerPatch Start");
    }
    [HarmonyPatch(nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void Update_Postfix(HudManager __instance)
    {
        try
        {
            GameObject gameStart = GameObject.Find("GameStartManager");
            if (gameStart != null)
                gameStart.transform.SetLocalY(-2.8f);

            if (GameStates.InGame && __instance?.Chat?.gameObject.active != true)
                __instance.Chat.gameObject.SetActive(true);

            GameObject TaskDisplay = GameObject.Find("TaskDisplay");
            GameObject ProgressTracker = GameObject.Find("ProgressTracker");
            if (TaskDisplay != null && ProgressTracker != null)
            {
                if (PlayerControl.LocalPlayer.IsImpostorTeam())
                {
                    __instance.CrewmatesKilled.gameObject.SetActive(true);
                    __instance.CrewmatesKilled.gameObject.transform.SetLocalZ(-25);
                    TaskDisplay.transform.localPosition = new Vector3(0f, -0.4f, -5f);
                    ProgressTracker.transform.localPosition = new Vector3(-2.8733f, 3.1f);
                }
                else if (__instance.CrewmatesKilled.isActiveAndEnabled)
                {
                    __instance.CrewmatesKilled.gameObject.SetActive(GameManager.Instance.ShowCrewmatesKilled());
                    TaskDisplay.transform.localPosition = new Vector3(0f, 0f);
                    ProgressTracker.transform.localPosition = new Vector3(-2.8733f, 2.7f, 0);
                }
            }
        }
        catch { }
    }
}

[HarmonyPatch(typeof(KillOverlay))]
public class KillOverlayPatch
{
    [HarmonyPatch(nameof(KillOverlay.ShowKillAnimation))]
    [HarmonyPrefix]
    private static bool ShowKillAnimation_Prefix()
    {
        if (!PlayerControl.LocalPlayer.IsAlive())
        {
            return false;
        }

        return true;
    }
}
