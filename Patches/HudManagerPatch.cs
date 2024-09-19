using HarmonyLib;
using TMPro;
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
        if (BetterNotificationManager.BAUNotificationManagerObj == null)
        {
            var ChatNotifications = __instance.Chat.chatNotification;
            if (ChatNotifications != null)
            {
                ChatNotifications.timeOnScreen = 1f;
                ChatNotifications.gameObject.SetActive(true);
                GameObject BAUNotification = UnityEngine.Object.Instantiate(ChatNotifications.gameObject);
                BAUNotification.name = "BAUNotification";
                UnityEngine.Object.Destroy(BAUNotification.GetComponent<ChatNotification>());
                UnityEngine.Object.Destroy(GameObject.Find($"{BAUNotification.name}/Sizer/PoolablePlayer"));
                UnityEngine.Object.Destroy(GameObject.Find($"{BAUNotification.name}/Sizer/ColorText"));
                BAUNotification.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(-1.57f, 5.3f, -15f);
                GameObject.Find($"{BAUNotification.name}/Sizer/NameText").transform.localPosition = new Vector3(-3.3192f, -0.0105f);
                BetterNotificationManager.NameText = GameObject.Find($"{BAUNotification.name}/Sizer/NameText").GetComponent<TextMeshPro>();
                UnityEngine.Object.DontDestroyOnLoad(BAUNotification);
                BetterNotificationManager.BAUNotificationManagerObj = BAUNotification;
                BAUNotification.SetActive(false);
                ChatNotifications.timeOnScreen = 0f;
                ChatNotifications.gameObject.SetActive(false);
            }
        }

        _ = new LateTask(() =>
        {
            if (!HasBeenWelcomed && GameStates.IsInGame && GameStates.IsLobby && !GameStates.IsFreePlay)
            {
                BetterNotificationManager.Notify("<b><color=#00751f>Welcome To Better Among Us!</color></b>", 8f);

                Utils.AddChatPrivate(WelcomeMessage, overrideName: " ");
                HasBeenWelcomed = true;
            }
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


            // Set chat
            if (GameStates.InGame)
            {
                if (!Main.ChatInGameplay.Value)
                {
                    if (!PlayerControl.LocalPlayer.IsAlive())
                    {
                        __instance.Chat.gameObject.SetActive(true);
                    }
                    else if (GameStates.IsInGamePlay && !(GameStates.IsMeeting || GameStates.IsExilling))
                    {
                        __instance.Chat.gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (__instance?.Chat?.gameObject.active == false)
                    {
                        __instance.Chat.gameObject.SetActive(true);
                    }
                }
            }
        }
        catch { }
    }
}

[HarmonyPatch(typeof(KillOverlay))]
public class KillOverlayPatch
{
    [HarmonyPatch(nameof(KillOverlay.ShowKillAnimation), new Type[] { typeof(OverlayKillAnimation), typeof(NetworkedPlayerInfo), typeof(NetworkedPlayerInfo) })]
    [HarmonyPrefix]
    public static bool ShowKillAnimation_Prefix()
    {
        if (!PlayerControl.LocalPlayer.IsAlive())
        {
            return false;
        }

        return true;
    }
}
