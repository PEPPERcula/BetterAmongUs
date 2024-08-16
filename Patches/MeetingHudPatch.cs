using HarmonyLib;
using System.Text;
using TMPro;
using UnityEngine;

namespace BetterAmongUs;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
class MeetingHudStartPatch
{
    // Set up meeting player info text
    public static void Postfix(MeetingHud __instance)
    {
        foreach (var pva in __instance.playerStates)
        {
            var TextTopMeeting = UnityEngine.Object.Instantiate(pva.NameText, pva.NameText.transform);
            TextTopMeeting.gameObject.name = "TextTop";
            TextTopMeeting.DestroyChildren();
            TextTopMeeting.transform.position = pva.NameText.transform.position;
            TextTopMeeting.transform.position += new Vector3(0f, 0.15f);
            TextTopMeeting.GetComponent<TextMeshPro>().text = "";

            var TextInfoMeeting = UnityEngine.Object.Instantiate(pva.NameText, pva.NameText.transform);
            TextInfoMeeting.gameObject.name = "TextInfo";
            TextInfoMeeting.DestroyChildren();
            TextInfoMeeting.transform.position = pva.NameText.transform.position;
            TextInfoMeeting.transform.position += new Vector3(0f, 0.28f);
            TextInfoMeeting.GetComponent<TextMeshPro>().text = "";

            var PlayerLevel = pva.transform.Find("PlayerLevel");
            PlayerLevel.localPosition = new Vector3(PlayerLevel.localPosition.x, PlayerLevel.localPosition.y, -2f);
            var LevelDisplay = UnityEngine.Object.Instantiate(PlayerLevel, pva.transform);
            LevelDisplay.transform.SetSiblingIndex(pva.transform.Find("PlayerLevel").GetSiblingIndex() + 1);
            LevelDisplay.gameObject.name = "PlayerId";
            LevelDisplay.GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 1f, 1f);
            var IdLabel = LevelDisplay.transform.Find("LevelLabel");
            var IdNumber = LevelDisplay.transform.Find("LevelNumber");
            UnityEngine.Object.Destroy(IdLabel.GetComponent<TextTranslatorTMP>());
            IdLabel.GetComponent<TextMeshPro>().text = "ID";
            IdNumber.GetComponent<TextMeshPro>().text = pva.TargetPlayerId.ToString();
            IdLabel.name = "IdLabel";
            IdNumber.name = "IdNumber";
            PlayerLevel.transform.position += new Vector3(0.23f, 0f);
        }

        RPC.SyncAllNames(true, true);

        Logger.LogHeader("Meeting Has Started");
    }
}
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
class MeetingHudUpdatePatch
{
    public static float timeOpen = 0f;

    // Set player meeting info
    public static void Postfix(MeetingHud __instance)
    {
        timeOpen += Time.deltaTime;

        foreach (var pva in __instance.playerStates)
        {
            if (pva == null) return;

            if (pva.ColorBlindName.isActiveAndEnabled)
            {
                pva.ColorBlindName.transform.localPosition = new Vector3(-0.91f, -0.19f, -0.05f);
                pva.ColorBlindName.color = Palette.PlayerColors[pva.PlayerIcon.ColorId];
                pva.ColorBlindName.outlineWidth = 0.2745f;
            }

            TextMeshPro TopText = pva.NameText.transform.Find("TextTop").gameObject.GetComponent<TextMeshPro>();
            TextMeshPro InfoText = pva.NameText.transform.Find("TextInfo").gameObject.GetComponent<TextMeshPro>();

            bool flag = Main.AllPlayerControls.Any(player => player.PlayerId == pva.TargetPlayerId);

            if (!flag)
            {
                SetPlayerTextInfoMeeting(pva, "<color=#6b6b6b>Disconnected</color>", isInfo: true);
                SetPlayerTextInfoMeeting(pva, "");
                pva.transform.Find("votePlayerBase").gameObject.SetActive(false);
                pva.transform.Find("deadX_border").gameObject.SetActive(false);
                pva.ClearForResults();
                pva.SetDisabled();
            }
            else if (TopText != null && InfoText != null)
            {
                var target = Utils.PlayerFromId(pva.TargetPlayerId);
                if (target == null) return;
                string hashPuid = Utils.GetHashPuid(target);
                string friendCode = target.Data.FriendCode;

                StringBuilder sbTag = new StringBuilder();
                StringBuilder sbInfo = new StringBuilder();

                // Put +++ at the end of each tag

                if (!string.IsNullOrEmpty(hashPuid) && AntiCheat.SickoData.ContainsKey(hashPuid) || !string.IsNullOrEmpty(friendCode) && AntiCheat.SickoData.ContainsValue(friendCode))
                    sbTag.Append("<color=#00f583>Sicko User</color>+++");
                else if (!string.IsNullOrEmpty(hashPuid) && AntiCheat.AUMData.ContainsKey(hashPuid) || !string.IsNullOrEmpty(friendCode) && AntiCheat.AUMData.ContainsValue(friendCode))
                    sbTag.Append("<color=#4f0000>AUM User</color>+++");
                else if (!string.IsNullOrEmpty(hashPuid) && AntiCheat.PlayerData.ContainsKey(hashPuid) || !string.IsNullOrEmpty(friendCode) && AntiCheat.PlayerData.ContainsValue(friendCode))
                    sbTag.Append("<color=#fc0000>Known Cheater</color>+++");

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
                    if (PlayerControl.LocalPlayer.IsAlive() && target != PlayerControl.LocalPlayer)
                    {
                        if (!DebugMenu.RevealRoles)
                        {
                            Role = "";
                        }
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

                SetPlayerTextInfoMeeting(pva, $"{sbInfo}", isInfo: true);
                SetPlayerTextInfoMeeting(pva, $"{Role}");
            }
        }
    }

    private static void SetPlayerTextInfoMeeting(PlayerVoteArea pva, string text, bool isInfo = false)
    {
        string InfoType = "TextTop";
        if (isInfo)
        {
            InfoType = "TextInfo";
            GameObject TopText = pva.NameText.transform.Find("TextTop").gameObject;
            if (TopText != null)
            {
                if (TopText.GetComponent<TextMeshPro>()?.text.Replace("<size=65%>", string.Empty).Replace("</size>", string.Empty).Length < 1)
                {
                    text = "<voffset=-2em>" + text + "</voffset>";
                }
            }
        }

        text = "<size=65%>" + text + "</size>";
        GameObject TextObj = pva.NameText.transform.Find(InfoType).gameObject; ;
        if (TextObj != null)
        {
            TextObj.GetComponent<TextMeshPro>().text = text;
        }
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.OnDestroy))]
class MeetingHud_OnDestroyPatch
{
    public static void Postfix()
    {
        MeetingHudUpdatePatch.timeOpen = 0f;
        Logger.LogHeader("Meeting Has Endded");
        RPC.SyncAllNames(force: true);
    }
}