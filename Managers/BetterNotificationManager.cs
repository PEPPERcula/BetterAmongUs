using BetterAmongUs.Data;
using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using BetterAmongUs.Patches;
using Cpp2IL.Core.Extensions;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Managers;

class BetterNotificationManager
{
    internal static GameObject? BAUNotificationManagerObj;
    internal static TextMeshPro? NameText;
    internal static TextMeshPro? TextArea => BAUNotificationManagerObj.transform.Find("Sizer/ChatText (TMP)").GetComponent<TextMeshPro>();
    internal static Dictionary<string, float> NotifyQueue = [];
    internal static float showTime = 0f;
    private static Camera? localCamera;
    internal static bool Notifying = false;

    internal static void Notify(string text, float Time = 5f)
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
            SoundManager.Instance.PlaySound(HudManager.Instance.TaskCompleteSound, false, 1f);
            Notifying = true;
        }
    }

    internal static void NotifyCheat(PlayerControl player, string reason, string newText = "", bool kickPlayer = true, bool forceBan = false)
    {
        if (player.IsLocalPlayer())
        {
            /*
            FileChecker.SetHasUnauthorizedFileOrMod();
            FileChecker.SetWarningMsg("Tampered client detected!");
            Utils.DisconnectSelf("Tampered client detected!");
            Utils.DisconnectAccountFromOnline();
            */
            return;
        }

        if (player?.Data == null) return;

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

        if (!BetterDataManager.BetterDataFile.CheatData.Any(info => info.CheckPlayerData(player.Data)))
        {
            BetterDataManager.BetterDataFile.CheatData.Add(new(player?.BetterData().RealName ?? player.Data.PlayerName, player.GetHashPuid(), player.Data.FriendCode, reason));
            BetterDataManager.BetterDataFile.Save();
            Notify(text, Time: 8f);
        }

        Logger.LogCheat($"{player.cosmetics.nameText.text} Info: {player.Data.PlayerName} - {player.Data.FriendCode} - {player.GetHashPuid()}");
        Logger.LogCheat(Utils.RemoveHtmlText(rawText));

        player.DirtyName();

        if (GameState.IsHost && kickPlayer)
        {
            string kickMessage = string.Format(Translator.GetString("AntiCheat.KickMessage"), byAntiCheat, Reason);
            player.Kick(true, kickMessage, true, false, forceBan);
        }
    }


    internal static void Update()
    {
        if (BAUNotificationManagerObj != null)
        {
            if (!localCamera)
            {
                if (HudManager.InstanceExists)
                {
                    localCamera = HudManager.Instance.GetComponentInChildren<Camera>();
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
