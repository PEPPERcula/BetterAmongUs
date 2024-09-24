using AmongUs.GameOptions;
using Assets.CoreScripts;
using HarmonyLib;
using Hazel;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Patches;

// History and dark mode from: https://github.com/0xDrMoe/TownofHost-Enhanced

class ChatPatch
{
    public static List<string> ChatHistory = [];
    public static int CurrentHistorySelection = -1;

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSendChat))]
    class RpcSendChatPatch
    {
        public static bool Prefix(PlayerControl __instance, string chatText, ref bool __result)
        {
            if (string.IsNullOrWhiteSpace(chatText))
            {
                __result = false;
                return false;
            }
            if (!GameStates.IsBetterHostLobby)
            {
                __result = false;
                return true;
            }
            chatText = Regex.Replace(chatText, "<.*?>", string.Empty);
            if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
            {
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(__instance, chatText, true);
            }
            if (chatText.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                DestroyableSingleton<UnityTelemetry>.Instance.SendWho();
            }
            chatText = "\n" + chatText;
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(__instance.NetId, 13, SendOption.Reliable);
            messageWriter.Write(chatText);
            messageWriter.EndMessage();
            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(ChatController))]
    public class ChatControllerPatch
    {
        [HarmonyPatch(nameof(ChatController.Toggle))]
        [HarmonyPostfix]
        public static void Toggle_Postfix(/*ChatController __instance*/)
        {
            SetChatTheme();
        }

        [HarmonyPatch(nameof(ChatController.Update))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        public static void Update_Prefix(ChatController __instance)
        {
            if (Main.ChatDarkMode.Value)
            {
                // Free chat color
                __instance.freeChatField.background.color = new Color32(40, 40, 40, byte.MaxValue);
                __instance.freeChatField.textArea.compoText.Color(Color.white);
                __instance.freeChatField.textArea.outputText.color = Color.white;
            }
            else
            {
                // Free chat color
                __instance.freeChatField.background.color = new Color32(255, 255, 255, byte.MaxValue);
                __instance.freeChatField.textArea.compoText.Color(Color.black);
                __instance.freeChatField.textArea.outputText.color = Color.black;
            }

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

        // Add extra information to chat bubble
        [HarmonyPatch(nameof(ChatController.AddChat))]
        [HarmonyPostfix]
        public static void AddChat_Postfix(ChatController __instance, [HarmonyArgument(0)] PlayerControl sourcePlayer, [HarmonyArgument(1)] string chatText)
        {
            ChatBubble chatBubble = SetChatPoolTheme();

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
                    sbTag.Append($"<color=#0088ff>Dev</color>+++");

                if (((sourcePlayer.IsLocalPlayer() && GameStates.IsHost && Main.BetterHost.Value)
                    || (!sourcePlayer.IsLocalPlayer() && sourcePlayer.BetterData().IsBetterHost && sourcePlayer.IsHost())))
                    sbTag.AppendFormat("<color=#0dff00>{1}{0}</color>+++", Translator.GetString("Player.BetterHost"), sourcePlayer.BetterData().IsVerifiedBetterUser || sourcePlayer.IsLocalPlayer() ? "✓ " : "");
                else if ((sourcePlayer.IsLocalPlayer() || sourcePlayer.BetterData().IsBetterUser))
                    sbTag.AppendFormat("<color=#0dff00>{1}{0}</color>+++", Translator.GetString("Player.BetterUser"), sourcePlayer.BetterData().IsVerifiedBetterUser || sourcePlayer.IsLocalPlayer() ? "✓ " : "");

                if (!string.IsNullOrEmpty(hashPuid) && AntiCheat.SickoData.ContainsKey(hashPuid) || !string.IsNullOrEmpty(friendCode) && AntiCheat.SickoData.ContainsValue(friendCode))
                    sbTag.Append($"<color=#00f583>{Translator.GetString("Player.SickoUser")}</color>+++");
                else if (!string.IsNullOrEmpty(hashPuid) && AntiCheat.AUMData.ContainsKey(hashPuid) || !string.IsNullOrEmpty(friendCode) && AntiCheat.AUMData.ContainsValue(friendCode))
                    sbTag.Append($"<color=#4f0000>{Translator.GetString("Player.AUMUser")}</color>+++");
                else if (!string.IsNullOrEmpty(hashPuid) && AntiCheat.PlayerData.ContainsKey(hashPuid) || !string.IsNullOrEmpty(friendCode) && AntiCheat.PlayerData.ContainsValue(friendCode))
                    sbTag.Append($"<color=#fc0000>{Translator.GetString("Player.KnownCheater")}</color>+++");
            }

            if (!sourcePlayer.IsImpostorTeammate())
            {
                if (PlayerControl.LocalPlayer.IsAlive() && !sourcePlayer.IsLocalPlayer())
                {
                    Role = "";
                }
            }

            if (PlayerControl.LocalPlayer.Is(RoleTypes.GuardianAngel) && !sourcePlayer.IsAlive() || !PlayerControl.LocalPlayer.Is(RoleTypes.GuardianAngel))
            {
                sbTag.Append(Role);
            }

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

            bool flag = sourcePlayer == PlayerControl.LocalPlayer;
            if (flag)
            {
                playerName = $"{sbInfo} " + playerName;
            }
            else
            {
                playerName += $" {sbInfo}";
            }

            chatBubble.NameText.text = playerName;
            chatBubble.ColorBlindName.color = Palette.PlayerColors[sourcePlayer.Data.DefaultOutfit.ColorId];
            Logger.Log($"{sourcePlayer.Data.PlayerName} -> {chatText}", "ChatLog");
        }

        [HarmonyPatch(nameof(ChatController.AddChatNote))]
        [HarmonyPostfix]
        public static void AddChatNote_Postfix(ChatController __instance)
        {
            SetChatPoolTheme();
        }

        [HarmonyPatch(nameof(ChatController.AddChatWarning))]
        [HarmonyPostfix]
        public static void AddChatWarning_Postfix(ChatController __instance)
        {
            SetChatPoolTheme();
        }

        public static void SetChatTheme()
        {
            var chat = HudManager.Instance.Chat;

            if (Main.ChatDarkMode.Value)
            {
                // Quick chat color
                chat.quickChatField.background.color = new Color32(40, 40, 40, byte.MaxValue);
                chat.quickChatField.text.color = Color.white;

                // Icons
                chat.quickChatButton.transform.Find("QuickChatIcon").GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
                chat.openKeyboardButton.transform.Find("OpenKeyboardIcon").GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }
            else
            {
                // Quick chat color
                chat.quickChatField.background.color = new Color32(255, 255, 255, byte.MaxValue);
                chat.quickChatField.text.color = Color.black;

                // Icons
                chat.quickChatButton.transform.Find("QuickChatIcon").GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
                chat.openKeyboardButton.transform.Find("OpenKeyboardIcon").GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
            }

            foreach (var item in HudManager.Instance.Chat.chatBubblePool.activeChildren.ToArray().Select(c => c.GetComponent<ChatBubble>()))
            {
                SetChatPoolTheme(item);
            }
        }

        // Set chat theme
        public static ChatBubble SetChatPoolTheme(ChatBubble? asChatBubble = null)
        {
            ChatBubble Get() => HudManager.Instance.Chat.chatBubblePool.activeChildren.ToArray()
                .Select(c => c.GetComponent<ChatBubble>())
                .Last();

            ChatBubble chatBubble = asChatBubble ??= Get();

            if (Main.ChatDarkMode.Value)
            {
                chatBubble.transform.Find("ChatText (TMP)").GetComponent<TextMeshPro>().color = new Color(1f, 1f, 1f, 1f);
                chatBubble.transform.Find("Background").GetComponent<SpriteRenderer>().color = new Color(0.05f, 0.05f, 0.05f, 1f);

                if (chatBubble.transform.Find("PoolablePlayer/xMark") != null)
                {
                    if (chatBubble.transform.Find("PoolablePlayer/xMark").GetComponent<SpriteRenderer>().enabled == true)
                    {
                        chatBubble.transform.Find("Background").GetComponent<SpriteRenderer>().color = new Color(0.05f, 0.05f, 0.05f, 0.5f);
                    }
                }
            }
            else
            {
                chatBubble.transform.Find("ChatText (TMP)").GetComponent<TextMeshPro>().color = new Color(0f, 0f, 0f, 1f);
                chatBubble.transform.Find("Background").GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);

                if (chatBubble.transform.Find("PoolablePlayer/xMark") != null)
                {
                    if (chatBubble.transform.Find("PoolablePlayer/xMark").GetComponent<SpriteRenderer>().enabled == true)
                    {
                        chatBubble.transform.Find("Background").GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
                    }
                }
            }

            return chatBubble;
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
            __instance.textArea.characterLimit = 118;
            __instance.charCountText.text = "0/118";
        }
        [HarmonyPatch(nameof(FreeChatInputField.UpdateCharCount))]
        [HarmonyPostfix]
        public static void UpdateCharCount_Postfix(FreeChatInputField __instance)
        {
            int length = __instance.textArea.text.Length;
            __instance.charCountText.text = string.Format("{0}/118", length);
            __instance.charCountText.color = GetCharColor(length, UnityEngine.Color.white);
        }
    }

    private static UnityEngine.Color GetCharColor(int length, UnityEngine.Color color)
    {

        switch (length)
        {
            case int n when n > 117:
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
