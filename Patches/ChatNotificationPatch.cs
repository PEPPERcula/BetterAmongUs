using HarmonyLib;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Patches;

[HarmonyPatch(typeof(ChatNotification))]
public class ChatNotificationPatch
{
    [HarmonyPatch(nameof(ChatNotification.Awake))]
    [HarmonyPostfix]
    public static void Awake_Postfix(ChatNotification __instance)
    {
        if (__instance.gameObject.name == "ChatNotification" && BetterNotificationManager.BAUNotificationManagerObj == null)
        {
            GameObject ChatNotifications = __instance.gameObject;
            if (ChatNotifications != null)
            {
                GameObject BAUNotification = UnityEngine.Object.Instantiate(ChatNotifications);
                BAUNotification.name = "BAUNotification";
                UnityEngine.Object.Destroy(BAUNotification.GetComponent<ChatNotification>());
                UnityEngine.Object.Destroy(GameObject.Find($"{BAUNotification.name}/Sizer/PoolablePlayer"));
                UnityEngine.Object.Destroy(GameObject.Find($"{BAUNotification.name}/Sizer/ColorText"));
                BAUNotification.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(-1.57f, 5.3f, -15f);
                GameObject.Find($"{BAUNotification.name}/Sizer/NameText").transform.localPosition = new Vector3(-3.3192f, -0.0105f);
                GameObject.Find($"{BAUNotification.name}/Sizer/NameText").GetComponent<TextMeshPro>().text = "<color=#00ff44>System Notification</color>";
                UnityEngine.Object.DontDestroyOnLoad(BAUNotification);
                BetterNotificationManager.BAUNotificationManagerObj = BAUNotification;
                BAUNotification.SetActive(false);
            }
        }
    }
    [HarmonyPatch(nameof(ChatNotification.Update))]
    [HarmonyPostfix]
    public static void Update_Postfix(ChatNotification __instance)
    {
        __instance.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(-2.8f, 0.3f, -40f);
        __instance.transform.localScale = new Vector3(0.45f, 0.42f);
    }
}
