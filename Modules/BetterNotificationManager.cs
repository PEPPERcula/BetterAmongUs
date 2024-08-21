using TMPro;
using UnityEngine;

namespace BetterAmongUs;

class BetterNotificationManager
{
    public static GameObject BAUNotificationManagerObj;
    public static Dictionary<string, float> NotifyQueue = [];
    public static float showTime = 0f;
    private static Camera localCamera;
    public static bool Notifying = false;

    public static void Notify(string text, float Time = 5f)
    {
        if (!Main.BetterNotifications.Value) return;

        if (BAUNotificationManagerObj != null)
        {
            if (Notifying) 
            {
                if (text == BAUNotificationManagerObj.transform.Find("Sizer/ChatText (TMP)").GetComponent<TextMeshPro>().text)
                    return;
                NotifyQueue[text] = Time;
                return;
            }

            showTime = Time;
            BAUNotificationManagerObj.SetActive(true);
            BAUNotificationManagerObj.transform.Find("Sizer/ChatText (TMP)").GetComponent<TextMeshPro>().text = text;
            SoundManager.Instance.PlaySound(DestroyableSingleton<HudManager>.Instance.TaskCompleteSound, false, 1f);
            Notifying = true;
        }
    }

    public static void NotifyCheat(PlayerControl player, string reason, string newText = "", bool kickPlayer = true)
    {
        string text = $"Player: <color=#0097b5>{player?.BetterData().RealName}</color> Has been detected doing an unauthorized action: <b><color=#fc0000>{reason}</color></b>";
        if (newText != "")
            text = $"Player: <color=#0097b5>{player?.BetterData().RealName}</color> " + newText + $": <b><color=#fc0000>{reason}</color></b>";

        if (!AntiCheat.PlayerData.ContainsKey(Utils.GetHashPuid(player)))
        {
            AntiCheat.PlayerData[Utils.GetHashPuid(player)] = player.Data.FriendCode;
            BetterDataManager.SaveCheatData(Utils.GetHashPuid(player), player.Data.FriendCode, player?.BetterData().RealName, "cheatData", reason);
            Notify(text, Time: 8f);
        }

        Logger.LogCheat(Utils.RemoveHtmlText(text));

        if (GameStates.IsHost && kickPlayer)
        {
            player.Kick(true, $"banned by <color=#4f92ff>Anti-Cheat</color>!\n Reason: <color=#fc0000>{reason}</color>", true);
        }
    }

    public static void Update()
    {
        if (BAUNotificationManagerObj != null)
        {
            if (!localCamera)
            {
                if (DestroyableSingleton<HudManager>.InstanceExists)
                {
                    localCamera = DestroyableSingleton<HudManager>.Instance.GetComponentInChildren<Camera>();
                }
                else
                {
                    localCamera = Camera.main;
                }
            }

            BAUNotificationManagerObj.transform.position = AspectPosition.ComputeWorldPosition(localCamera, AspectPosition.EdgeAlignments.Bottom, new Vector3(-1.3f, 0.7f, localCamera.nearClipPlane + 0.1f));

            showTime -= Time.deltaTime;
            if (showTime <= 0f && GameStates.IsInGame)
            {
                BAUNotificationManagerObj.transform.Find("Sizer/ChatText (TMP)").GetComponent<TextMeshPro>().text = "";
                BAUNotificationManagerObj.SetActive(false);
                Notifying = false;

                CheckNotifyQueue();
            }

            if (!GameStates.IsInGame)
            {
                BAUNotificationManagerObj.SetActive(false);
                showTime = 0f;
            }
        }
    }

    private static void CheckNotifyQueue()
    {
        if (NotifyQueue.Any())
        {
            var key = NotifyQueue.Keys.First();
            var value = NotifyQueue[key];
            Notify(key, value);
            NotifyQueue.Remove(key);
        }
    }
}
