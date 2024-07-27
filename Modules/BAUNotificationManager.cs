using TMPro;
using UnityEngine;

namespace BetterAmongUs;

class BAUNotificationManager
{
    public static GameObject BAUNotificationManagerObj;
    private static Dictionary<string, float> NotifyQueue = [];
    private static float showTime = 0f;
    private static Camera localCamera;
    private static bool Notifying = false;

    public static void Notify(string text, float Time = 5f)
    {
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
        string text = $"Player: <color=#0097b5>{player?.CurrentOutfit.PlayerName}</color> Has been detected doing an unauthorized action: <b><color=#fc0000>{reason}</color></b>";
        if (newText != "")
            text = $"Player: <color=#0097b5>{player?.CurrentOutfit.PlayerName}</color> " + newText + $": <b><color=#fc0000>{reason}</color></b>";

        if (!AntiCheat.PlayerData.ContainsKey(Utils.GetHashPuid(player)))
        {
            AntiCheat.PlayerData[Utils.GetHashPuid(player)] = player.Data.FriendCode;
            BAUDataManager.SaveCheatData(Utils.GetHashPuid(player), player.Data.FriendCode, player.Data.PlayerName, "cheatData", reason);
            Notify(text, Time: 8f);
        }

        if (GameStates.IsHost && kickPlayer)
        {
            player.RpcSetName($"<color=#ffea00>{player.Data.PlayerName}</color> Has been banned by <color=#4f92ff>Anti-Cheat</color>, Reason: {reason}<size=0%>");
            AmongUsClient.Instance.KickPlayer(player.GetClientId(), true);
        }

        Logger.LogCheat(Utils.GetRawText(text));
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
            if (showTime <= 0f)
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
