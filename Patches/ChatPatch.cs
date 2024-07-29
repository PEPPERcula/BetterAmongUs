using AmongUs.Data;
using HarmonyLib;
using System.Text;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Patches;

// History and dark mode from: https://github.com/0xDrMoe/TownofHost-Enhanced

class ChatPatch
{
    public static List<string> ChatHistory = [];
    public static int CurrentHistorySelection = -1;

    [HarmonyPatch(typeof(ChatController))]
    class ChatControllerPatch
    {
        [HarmonyPatch(nameof(ChatController.Update))]
        [HarmonyPostfix]
        public static void Update_Postfix(ChatController __instance)
        {
            for (int i = 0; i < __instance.scroller.Inner.gameObject.transform.childCount; i++)
            {
                GameObject chatItem = __instance.scroller.Inner.transform.GetChild(i).gameObject;

                chatItem.transform.Find("Background").GetComponent<SpriteRenderer>().color = new Color(0.05f, 0.05f, 0.05f, 1f);
                chatItem.transform.Find("ChatText (TMP)").GetComponent<TextMeshPro>().color = new Color(1f, 1f, 1f, 1f);
            }

            // Free chat color
            __instance.freeChatField.background.color = new Color32(40, 40, 40, byte.MaxValue);
            __instance.freeChatField.textArea.compoText.Color(Color.white);
            __instance.freeChatField.textArea.outputText.color = Color.white;

            // Quick chat color
            __instance.quickChatField.background.color = new Color32(40, 40, 40, byte.MaxValue);
            __instance.quickChatField.text.color = Color.white;

            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.X))
            {
                ClipboardHelper.PutClipboardString(__instance.freeChatField.textArea.text);
                __instance.freeChatField.textArea.SetText("");
            }

            if (Input.GetKeyDown(KeyCode.UpArrow) && ChatHistory.Any())
            {
                CurrentHistorySelection = Mathf.Clamp(--CurrentHistorySelection, 0, ChatHistory.Count - 1);
                __instance.freeChatField.textArea.SetText(ChatHistory[CurrentHistorySelection]);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow) && ChatHistory.Any())
            {
                CurrentHistorySelection++;
                if (CurrentHistorySelection < ChatHistory.Count)
                    __instance.freeChatField.textArea.SetText(ChatHistory[CurrentHistorySelection]);
                else __instance.freeChatField.textArea.SetText("");
            }
        }

        [HarmonyPatch(nameof(ChatController.AddChat))]
        [HarmonyPrefix]
        public static bool AddChat_Prefix(ChatController __instance, [HarmonyArgument(0)] PlayerControl sourcePlayer, [HarmonyArgument(1)] string chatText)
        {
            bool censor = false;
            if (!sourcePlayer || !PlayerControl.LocalPlayer)
            {
                return false;
            }
            NetworkedPlayerInfo data = PlayerControl.LocalPlayer.Data;
            NetworkedPlayerInfo data2 = sourcePlayer.Data;
            if (data2 == null || data == null || (data2.IsDead && !data.IsDead))
            {
                return false;
            }
            ChatBubble pooledBubble = __instance.GetPooledBubble();
            try
            {
                pooledBubble.transform.SetParent(__instance.scroller.Inner);
                pooledBubble.transform.localScale = Vector3.one;
                bool flag = sourcePlayer == PlayerControl.LocalPlayer;
                if (flag)
                {
                    pooledBubble.SetRight();
                }
                else
                {
                    pooledBubble.SetLeft();
                }
                bool didVote = MeetingHud.Instance && MeetingHud.Instance.DidVote(sourcePlayer.PlayerId);
                pooledBubble.Background.color = new Color(0.05f, 0.05f, 0.05f, 1f);
                pooledBubble.SetCosmetics(data2);
                __instance.SetChatBubbleName(pooledBubble, data2, data2.IsDead, didVote, PlayerNameColor.Get(data2), null);
                if (censor && DataManager.Settings.Multiplayer.CensorChat)
                {
                    chatText = BlockedWords.CensorWords(chatText, false);
                }
                pooledBubble.SetText(chatText);
                pooledBubble.AlignChildren();
                __instance.AlignAllBubbles();
                if (!__instance.IsOpenOrOpening && __instance.notificationRoutine == null)
                {
                    __instance.notificationRoutine = __instance.StartCoroutine(__instance.BounceDot());
                }
                if (!flag)
                {
                    SoundManager.Instance.PlaySound(__instance.messageSound, false, 1f, null).pitch = 0.5f + (float)sourcePlayer.PlayerId / 15f;
                    __instance.chatNotification.SetUp(sourcePlayer, chatText);
                }

                StringBuilder sbTag = new StringBuilder();
                StringBuilder sbInfo = new StringBuilder();

                string hashPuid = Utils.GetHashPuid(sourcePlayer);
                string friendCode = sourcePlayer.Data.FriendCode;
                string playerName = sourcePlayer.Data.PlayerName;
                string Role = $"<size=75%><color={sourcePlayer.GetTeamHexColor()}>{sourcePlayer.GetRoleName()}</color></size>+++";

                if (GameStates.IsLobby && !GameStates.IsFreePlay)
                {
                    Role = "";

                    if (sourcePlayer.IsDev())
                        sbTag.Append("<color=#0088ff>Dev</color>+++");

                    if (((sourcePlayer == PlayerControl.LocalPlayer && GameStates.IsHost && Main.BetterHost.Value) || sourcePlayer.GetIsBetterHost() == true) && !GameStates.IsInGamePlay)
                        sbTag.Append("<color=#0dff00>Better Host</color>+++");
                    if ((sourcePlayer == PlayerControl.LocalPlayer || sourcePlayer.GetIsBetterUser() == true) && !GameStates.IsInGamePlay)
                        sbTag.Append("<color=#0dff00>Better User</color>+++");

                    if (!string.IsNullOrEmpty(hashPuid) && AntiCheat.SickoData.ContainsKey(hashPuid) || !string.IsNullOrEmpty(friendCode) && AntiCheat.SickoData.ContainsValue(friendCode))
                        sbTag.Append("<color=#00f583>Sicko User</color>+++");
                    else if (!string.IsNullOrEmpty(hashPuid) && AntiCheat.AUMData.ContainsKey(hashPuid) || !string.IsNullOrEmpty(friendCode) && AntiCheat.AUMData.ContainsValue(friendCode))
                        sbTag.Append("<color=#4f0000>AUM User</color>+++");
                    else if (!string.IsNullOrEmpty(hashPuid) && AntiCheat.PlayerData.ContainsKey(hashPuid) || !string.IsNullOrEmpty(friendCode) && AntiCheat.PlayerData.ContainsValue(friendCode))
                        sbTag.Append("<color=#fc0000>Known Cheater</color>+++");
                }

                if (!sourcePlayer.IsImpostorTeammate())
                {
                    if (PlayerControl.LocalPlayer.IsAlive() && sourcePlayer != PlayerControl.LocalPlayer)
                    {
                        Role = "";
                    }
                }

                sbTag.Append(Role);

                sbInfo.Append("<size=75%>");
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
                sbInfo.Append("</size>");

                if (flag)
                {
                    playerName = $"{sbInfo} " + playerName;
                }
                else
                {
                    playerName += $" {sbInfo}";
                }

                pooledBubble.NameText.SetText(playerName);

                Logger.Log($"{sourcePlayer.Data.PlayerName} -> {chatText}", "ChatLog");
            }
            catch
            {
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(FreeChatInputField))]
    class FreeChatInputFieldPatch
    {
        [HarmonyPatch(nameof(FreeChatInputField.Awake))]
        [HarmonyPostfix]
        public static void Awake_Postfix(FreeChatInputField __instance)
        {
            __instance.textArea.allowAllCharacters = true;
            __instance.textArea.AllowSymbols = true;
            __instance.textArea.AllowPaste = true;
            __instance.textArea.AllowEmail = true;
            __instance.textArea.characterLimit = 120;
            __instance.charCountText.text = "0/120";
        }
        [HarmonyPatch(nameof(FreeChatInputField.UpdateCharCount))]
        [HarmonyPostfix]
        public static void UpdateCharCount_Postfix(FreeChatInputField __instance)
        {
            int length = __instance.textArea.text.Length;
            __instance.charCountText.text = string.Format("{0}/120", length);
            __instance.charCountText.color = GetCharColor(length, UnityEngine.Color.white);
        }
    }

    private static UnityEngine.Color GetCharColor(int length, UnityEngine.Color color)
    {

        switch (length)
        {
            case int n when n > 119:
                if (ColorUtility.TryParseHtmlString("#ff0000", out UnityEngine.Color newColor1))
                    color = newColor1;
                break;
            case int n when n > 74:
                if (ColorUtility.TryParseHtmlString("#ffff00", out UnityEngine.Color newColor3))
                    color = newColor3;
                break;
            default:
                if (ColorUtility.TryParseHtmlString("#00f04c", out UnityEngine.Color newColor4))
                    color = newColor4;
                break;
        }

        return color;
    }
}
