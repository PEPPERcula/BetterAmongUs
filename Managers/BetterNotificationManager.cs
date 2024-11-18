using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using BetterAmongUs.Patches;
using Cpp2IL.Core.Extensions;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Managers;

class BetterNotificationManager
{
    public static GameObject? BAUNotificationManagerObj;
    public static TextMeshPro? NameText;
    public static TextMeshPro? TextArea => BAUNotificationManagerObj.transform.Find("Sizer/ChatText (TMP)").GetComponent<TextMeshPro>();
    public static Dictionary<string, float> NotifyQueue = [];
    public static float showTime = 0f;
    private static Camera? localCamera;
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
            NameText.text = $"<color=#00ff44>{Translator.GetString("SystemNotification")}</color>";
            TextArea.text = text;
            SoundManager.Instance.PlaySound(DestroyableSingleton<HudManager>.Instance.TaskCompleteSound, false, 1f);
            Notifying = true;
        }
    }

    public static void NotifyCheat(PlayerControl player, string reason, string newText = "", bool kickPlayer = true)
    {
        var Reason = reason;
        if (BetterGameSettings.CensorDetectionReason.GetBool())
        {
            Reason = string.Concat('*').Repeat(reason.Length);
        }

        string playerDetected = Translator.GetString("AntiCheat.PlayerDetected");
        string unauthorizedAction = Translator.GetString("AntiCheat.UnauthorizedAction");
        string byAntiCheat = Translator.GetString("AntiCheat.ByAntiCheat");
        string playerDetectedLog = Translator.GetString("AntiCheat.PlayerDetected", console: true);
        string unauthorizedActionLog = Translator.GetString("AntiCheat.UnauthorizedAction", console: true);

        string text = $"{playerDetected}: <color=#0097b5>{player?.BetterData().RealName}</color> {unauthorizedAction}: <b><color=#fc0000>{Reason}</color></b>";
        string rawText = $"{playerDetectedLog}: <color=#0097b5>{player?.BetterData().RealName}</color> {unauthorizedActionLog}: <b><color=#fc0000>{reason}</color></b>";

        if (newText != "")
        {
            text = $"{playerDetected}: <color=#0097b5>{player?.BetterData().RealName}</color> " + newText + $": <b><color=#fc0000>{Reason}</color></b>";
            rawText = $"{playerDetectedLog}: <color=#0097b5>{player?.BetterData().RealName}</color> " + newText + $": <b><color=#fc0000>{reason}</color></b>";
        }

        if (!AntiCheat.PlayerData.ContainsKey(Utils.GetHashPuid(player)))
        {
            AntiCheat.PlayerData[Utils.GetHashPuid(player)] = player.Data.FriendCode;
            BetterDataManager.SaveCheatData(Utils.GetHashPuid(player), player.Data.FriendCode, player?.BetterData().RealName, "cheatData", Reason);
            Notify(text, Time: 8f);
        }

        Logger.LogCheat(Utils.RemoveHtmlText(rawText));

        if (GameState.IsHost && kickPlayer)
        {
            string kickMessage = string.Format(Translator.GetString("AntiCheat.KickMessage"), byAntiCheat, Reason);
            player.Kick(true, kickMessage, true);
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
            if (showTime <= 0f && GameState.IsInGame)
            {
                BAUNotificationManagerObj.transform.Find("Sizer/ChatText (TMP)").GetComponent<TextMeshPro>().text = "";
                BAUNotificationManagerObj.SetActive(false);
                Notifying = false;

                CheckNotifyQueue();
            }

            if (!GameState.IsInGame)
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
