using AmongUs.GameOptions;
using BetterAmongUs.Data;
using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using BetterAmongUs.Patches;
using HarmonyLib;
using System.Text;
using TMPro;
using UnityEngine;

namespace BetterAmongUs;

[HarmonyPatch(typeof(MeetingHud))]
class MeetingHudPatch
{
    [HarmonyPatch(nameof(MeetingHud.Start))]
    [HarmonyPostfix]
    internal static void Start_Postfix(MeetingHud __instance)
    {
        TopText.Clear();
        InfoText.Clear();

        foreach (var pva in __instance.playerStates)
        {
            pva.ColorBlindName.transform.localPosition = new Vector3(-0.91f, -0.19f, -0.05f);

            var TextTopMeeting = UnityEngine.Object.Instantiate(pva.NameText, pva.NameText.transform);
            TextTopMeeting.gameObject.name = "TextTop";
            TextTopMeeting.DestroyChildren();
            TextTopMeeting.transform.position = pva.NameText.transform.position;
            TextTopMeeting.transform.position += new Vector3(0f, 0.15f);
            TextTopMeeting.text = "";
            TopText[pva.TargetPlayerId] = TextTopMeeting;

            var TextInfoMeeting = UnityEngine.Object.Instantiate(pva.NameText, pva.NameText.transform);
            TextInfoMeeting.gameObject.name = "TextInfo";
            TextInfoMeeting.DestroyChildren();
            TextInfoMeeting.transform.position = pva.NameText.transform.position;
            TextInfoMeeting.transform.position += new Vector3(0f, 0.28f);
            TextInfoMeeting.text = "";
            InfoText[pva.TargetPlayerId] = TextInfoMeeting;

            var PlayerLevel = pva.transform.Find("PlayerLevel");
            PlayerLevel.localPosition = new Vector3(PlayerLevel.localPosition.x, PlayerLevel.localPosition.y, -2f);
            var LevelDisplay = UnityEngine.Object.Instantiate(PlayerLevel, pva.transform);
            LevelDisplay.transform.SetSiblingIndex(pva.transform.Find("PlayerLevel").GetSiblingIndex() + 1);
            LevelDisplay.gameObject.name = "PlayerId";
            LevelDisplay.GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 1f, 1f);
            var IdLabel = LevelDisplay.transform.Find("LevelLabel");
            var IdNumber = LevelDisplay.transform.Find("LevelNumber");
            IdLabel.gameObject.DestroyTextTranslator();
            IdLabel.GetComponent<TextMeshPro>().text = "ID";
            IdNumber.GetComponent<TextMeshPro>().text = pva.TargetPlayerId.ToString();
            IdLabel.name = "IdLabel";
            IdNumber.name = "IdNumber";
            PlayerLevel.transform.position += new Vector3(0.23f, 0f);
        }

        Logger.LogHeader("Meeting Has Started");
    }

    [HarmonyPatch(nameof(MeetingHud.SetMasksEnabled))]
    [HarmonyPostfix]
    internal static void SetMasksEnabled_Postfix(/*MeetingHud __instance*/)
    {
        Utils.DirtyAllNames();
    }

    internal static Dictionary<byte, TextMeshPro?> TopText = [];
    internal static Dictionary<byte, TextMeshPro?> InfoText = [];
    internal static float timeOpen = 0f;

    // Set player meeting info
    [HarmonyPatch(nameof(MeetingHud.Update))]
    [HarmonyPostfix]
    internal static void Update_Postfix(MeetingHud __instance)
    {
        timeOpen += Time.deltaTime;

        foreach (var pva in __instance.playerStates)
        {
            if (pva == null) continue;

            bool flag = Main.AllPlayerControls.Any(pc => pc.PlayerId == pva.TargetPlayerId);

            if (!flag && TopText != null && InfoText != null)
            {
                string DisconnectText;
                var playerData = GameData.Instance.GetPlayerById(pva.TargetPlayerId);
                switch (playerData.BetterData().DisconnectReason)
                {
                    case DisconnectReasons.ExitGame:
                        DisconnectText = Translator.GetString("DisconnectReasonMeeting.Left");
                        break;
                    case DisconnectReasons.Banned:
                        if (playerData.BetterData().AntiCheatInfo.BannedByAntiCheat)
                        {
                            DisconnectText = Translator.GetString("DisconnectReasonMeeting.AntiCheat");
                        }
                        else
                        {
                            DisconnectText = Translator.GetString("DisconnectReasonMeeting.Banned");
                        }
                        break;
                    case DisconnectReasons.Kicked:
                        DisconnectText = Translator.GetString("DisconnectReasonMeeting.Kicked");
                        break;
                    case DisconnectReasons.Hacking:
                        DisconnectText = Translator.GetString("DisconnectReasonMeeting.Cheater");
                        break;
                    default:
                        DisconnectText = Translator.GetString("DisconnectReasonMeeting.Disconnect");
                        break;
                }

                SetPlayerTextInfoMeeting(pva, $"<color=#6b6b6b>{DisconnectText}</color>", isInfo: true);
                SetPlayerTextInfoMeeting(pva, "");
                pva.transform.Find("votePlayerBase").gameObject.SetActive(false);
                pva.transform.Find("deadX_border").gameObject.SetActive(false);
                pva.ClearForResults();
                pva.SetDisabled();
            }
            else if (TopText != null && InfoText != null)
            {
                var target = Utils.PlayerFromPlayerId(pva.TargetPlayerId);
                if (target == null) continue;
                if (target?.BetterData()?.IsDirtyInfo != true) continue;
                target.BetterData().IsDirtyInfo = false;

                string hashPuid = Utils.GetHashPuid(target);
                string friendCode = target.Data.FriendCode;

                StringBuilder sbTag = new StringBuilder();
                StringBuilder sbInfo = new StringBuilder();

                // Put +++ at the end of each tag

                if (BetterDataManager.BetterDataFile.SickoData.Any(info => info.CheckPlayerData(target.Data)))
                    sbTag.Append($"<color=#00f583>{Translator.GetString("Player.SickoUser")}</color>+++");
                else if (BetterDataManager.BetterDataFile.AUMData.Any(info => info.CheckPlayerData(target.Data)))
                    sbTag.Append($"<color=#4f0000>{Translator.GetString("Player.AUMUser")}</color>+++");
                else if (BetterDataManager.BetterDataFile.KNData.Any(info => info.CheckPlayerData(target.Data)))
                    sbTag.Append($"<color=#8731e7>{Translator.GetString("Player.KNUser")}</color>+++");
                else if (BetterDataManager.BetterDataFile.CheatData.Any(info => info.CheckPlayerData(target.Data)))
                    sbTag.Append($"<color=#fc0000>{Translator.GetString("Player.KnownCheater")}</color>+++");

                for (int i = 0; i < sbTag.ToString().Split("+++").Length; i++)
                {
                    if (!string.IsNullOrEmpty(sbTag.ToString().Split("+++")[i]))
                    {
                        if (i < sbTag.ToString().Split("+++").Length)
                        {
                            sbInfo.Append(sbTag.ToString().Split("+++")[i]);
                        }
                        if (i != sbTag.ToString().Split("+++").Length - 2)
                        {
                            sbInfo.Append(" - ");
                        }
                    }
                }

                string RoleHexColor = target.IsImpostorTeam() ? "#ff1919" : "#8cffff";
                string Role = $"<color={RoleHexColor}>{target.GetRoleName()}</color>";
                if (!target.IsImpostorTeam() && target.myTasks.Count > 0)
                {
                    Role += $" <color=#cbcbcb>({target.myTasks.ToArray().Where(task => task.IsComplete).Count()}/{target.myTasks.Count})</color>";
                }
                if (!target.IsImpostorTeammate())
                {
                    if ((PlayerControl.LocalPlayer.IsAlive() || PlayerControl.LocalPlayer.Is(RoleTypes.GuardianAngel)) && !target.IsLocalPlayer())
                    {
                        Role = "";
                    }
                }

                Vector3 textPos;

                if (Role != "" && !string.IsNullOrEmpty(sbInfo.ToString()))
                    textPos = new Vector3(pva.NameText.transform.localPosition.x, -0.045f);
                else if (Role != "" || !string.IsNullOrEmpty(sbInfo.ToString()))
                    textPos = new Vector3(pva.NameText.transform.localPosition.x, -0.03f);
                else
                    textPos = new Vector3(pva.NameText.transform.localPosition.x, 0.015f);

                pva.NameText.transform.localPosition = textPos;

                SetPlayerTextInfoMeeting(pva, $"{sbInfo}", true);
                SetPlayerTextInfoMeeting(pva, $"{Role}");
            }
        }
    }

    private static void SetPlayerTextInfoMeeting(PlayerVoteArea pva, string text, bool isInfo = false)
    {
        if (isInfo)
        {
            if (TopText[pva.TargetPlayerId].text.Replace("<size=65%>", string.Empty).Replace("</size>", string.Empty).Length < 1)
            {
                text = "<voffset=-2em>" + text + "</voffset>";
            }
        }

        text = "<size=65%>" + text + "</size>";

        if (!isInfo)
        {
            TopText[pva.TargetPlayerId].text = text;
        }
        else
        {
            InfoText[pva.TargetPlayerId].text = text;
        }
    }

    [HarmonyPatch(nameof(MeetingHud.Close))]
    [HarmonyPostfix]
    internal static void Close_Postfix()
    {
        timeOpen = 0f;
        Logger.LogHeader("Meeting Has Ended");

        if (Main.ChatInGameplay.Value && !GameState.IsFreePlay && PlayerControl.LocalPlayer.IsAlive())
        {
            ChatPatch.ClearChat();
        }
    }
}