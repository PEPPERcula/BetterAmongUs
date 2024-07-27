using AmongUs.Data;
using HarmonyLib;
using Hazel;
using Il2CppSystem.Linq;
using LibCpp2IL;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Patches;

[HarmonyPatch(typeof(PlayerControl))]
class PlayerControlPatch
{
    public static float infotime = 0f;
    [HarmonyPatch(nameof(PlayerControl.FixedUpdate))]
    [HarmonyPostfix]
    public static void FixedUpdate_Postfix(PlayerControl __instance)
    {
        // Set up player text info
        var NameText = GameObject.Find($"{__instance.gameObject.transform.name}/Names/NameText_TMP");
        var InfoText = GameObject.Find($"{__instance.gameObject.transform.name}/Names/NameText_TMP/InfoText_T_TMP");
        if (NameText != null && InfoText == null)
        {
            GameObject playerInfoDisplayInfo = UnityEngine.Object.Instantiate(NameText, NameText.transform);
            playerInfoDisplayInfo.name = "InfoText_Info_TMP";
            playerInfoDisplayInfo.transform.DestroyChildren();
            playerInfoDisplayInfo.transform.position += new Vector3(0f, 0.25f);
            playerInfoDisplayInfo.GetComponent<TextMeshPro>().text = string.Empty;
            playerInfoDisplayInfo.SetActive(true);

            GameObject playerInfoDisplayTop = UnityEngine.Object.Instantiate(NameText, NameText.transform);
            playerInfoDisplayTop.name = "InfoText_T_TMP";
            playerInfoDisplayTop.transform.DestroyChildren();
            playerInfoDisplayTop.transform.position += new Vector3(0f, 0.15f);
            playerInfoDisplayTop.GetComponent<TextMeshPro>().text = string.Empty;
            playerInfoDisplayTop.SetActive(true);

            GameObject playerInfoDisplayBottom = UnityEngine.Object.Instantiate(NameText, NameText.transform);
            playerInfoDisplayBottom.name = "InfoText_B_TMP";
            playerInfoDisplayBottom.transform.DestroyChildren();
            playerInfoDisplayBottom.transform.position += new Vector3(0f, -0.15f);
            playerInfoDisplayBottom.GetComponent<TextMeshPro>().text = string.Empty;
            playerInfoDisplayBottom.SetActive(true);
        }

        // Set color blind text on player
        if (__instance.DataIsCollected() && !__instance.shapeshifting)
        {
            __instance.cosmetics.SetColorBlindColor(__instance.CurrentOutfit.ColorId);
        }
        else
        {
            __instance.cosmetics.colorBlindText.text = string.Empty;
        }

        infotime -= Time.deltaTime;

        if (infotime <= 0)
        {
            SetPlayerInfo(__instance);
            infotime = 0.4f;
        }

        if (GameStates.IsInGame && GameStates.IsHost)
            RPC.SyncAllNames(isBetterHost: false);
    }

    public static void SetPlayerInfo(PlayerControl player)
    {
        if (player == null) return;

        // Set player text info
        if (player.DataIsCollected() != true)
        {
            if (player.gameObject.transform.Find("Names/NameText_TMP").GetComponent<TextMeshPro>().text is "???" or "Player")
            {
                player.gameObject.transform.Find("Names/NameText_TMP").GetComponent<TextMeshPro>().text = "<color=#b5b5b5>Loading</color>";
            }

            return;
        }

        if (Main.AllPlayerControls.Any(pc => player.Data.PlayerId == pc.shapeshiftTargetPlayerId))
            return;

        string NewName = player.Data.PlayerName;
        string hashPuid = Utils.GetHashPuid(player);
        string friendCode = player.Data.FriendCode;
        int playerId = player.PlayerId;
        string Lv = " - <color=#ffd829>Lv: " + player.Data.PlayerLevel.ToString() + "</color>";

        string pattern = @"^[a-zA-Z0-9#]+$";
        string hashtagPattern = @"^#[0-9]{4}$";

        string friendCodeColor = (Regex.Replace(friendCode, hashtagPattern, string.Empty).Length is > 10 or < 5 || !Regex.IsMatch(friendCode, pattern) || !Regex.IsMatch(friendCode, hashtagPattern)) ? "#00f7ff" : "#ff0000";

        if (string.IsNullOrEmpty(friendCode) || friendCode == "")
            friendCode = "???";

        if (DataManager.Settings.Gameplay.StreamerMode == true)
            friendCode = string.Concat('*').Repeat(friendCode.Length);

        StringBuilder sbTag = new StringBuilder();
        StringBuilder sbInfo = new StringBuilder();

        // Put +++ at the end of each tag

        if (player.IsDev() && !GameStates.IsInGamePlay)
            sbTag.Append("<color=#6e6e6e>(<color=#0088ff>Dev</color>)</color>+++");

        if ((player == PlayerControl.LocalPlayer || player.GetIsBetterUser() == true) && GameStates.IsLobby && !GameStates.IsFreePlay)
            sbTag.Append("<color=#0dff00>Better User</color>+++");

        if (!string.IsNullOrEmpty(hashPuid) && AntiCheat.SickoData.ContainsKey(hashPuid) || !string.IsNullOrEmpty(friendCode) && AntiCheat.SickoData.ContainsValue(friendCode))
            sbTag.Append("<color=#00f583>Sicko User</color>+++");
        else if (!string.IsNullOrEmpty(hashPuid) && AntiCheat.AUMData.ContainsKey(hashPuid) || !string.IsNullOrEmpty(friendCode) && AntiCheat.AUMData.ContainsValue(friendCode))
            sbTag.Append("<color=#4f0000>AUM User</color>+++");
        else if (!string.IsNullOrEmpty(hashPuid) && AntiCheat.PlayerData.ContainsKey(hashPuid) || !string.IsNullOrEmpty(friendCode) && AntiCheat.PlayerData.ContainsValue(friendCode))
            sbTag.Append("<color=#fc0000>Known Cheater</color>+++");

        if (GameStates.IsLobby && !GameStates.IsFreePlay)
            sbTag.Append($"<color=#b554ff>ID: {playerId}</color>+++");

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

        PlayerControl Host = AmongUsClient.Instance.GetHost().Character;
        if (player.IsHost() && GameStates.InGame && GameStates.IsLobby && !GameStates.IsFreePlay)
        {
            if (Main.LobbyPlayerInfo.Value == true)
            {
                string hexColor = Utils.Color32ToHex(Palette.PlayerColors[Host.CurrentOutfit.ColorId]);
                NewName = $"<color={hexColor}>{player.Data.PlayerName}</color>";
            }
        }
        if (player.IsImpostorTeammate() && player != PlayerControl.LocalPlayer)
        {
            NewName = $"<color=#ff1919>{player.Data.PlayerName}</color>";
        }

        if (!player.IsInShapeshift())
            player.RawSetName(NewName);

        if (!GameStates.IsVanillaServer)
        {
            friendCode = string.Empty;
            Lv = string.Empty;
        }

        StringBuilder sbTag2 = new StringBuilder();
        StringBuilder sbInfo2 = new StringBuilder();

        // Put +++ at the end of each tag

        sbTag2.Append($"<color=#9e9e9e>{Utils.GetPlatformName(player, useTag: true)}</color>"); // Don't put +++ here
        sbTag2.Append($"{Lv}+++");

        for (int i = 0; i < sbTag2.ToString().Split("+++").Length; i++)
        {
            if (!string.IsNullOrEmpty(sbTag2.ToString().Split("+++")[i]))
            {
                if (i < sbTag2.ToString().Split("+++").Length)
                {
                    sbInfo2.Append(sbTag2.ToString().Split("+++")[i]);
                }
                if (i != sbTag2.ToString().Split("+++").Length - 2)
                {
                    sbInfo2.Append(" - ");
                }
            }
        }

        if (!GameStates.IsInGamePlay)
        {
            if (Main.LobbyPlayerInfo.Value == true)
            {
                player.SetPlayerTextInfo($"{sbInfo}", isInfo: true);
                player.SetPlayerTextInfo($"{sbInfo2}");
                player.SetPlayerTextInfo($"<color={friendCodeColor}>{friendCode}</color>", isBottom: true);
            }
            else
            {
                player.ResetAllPlayerTextInfo();
            }
        }
        else
        {
            string Role = $"<color={player.GetTeamHexColor()}>{player.GetRoleName()}</color>";
            if (!player.IsImpostorTeammate())
            {
                if (PlayerControl.LocalPlayer.IsAlive() && player != PlayerControl.LocalPlayer)
                {
                    if (!DebugMenu.RevealRoles)
                    {
                        Role = "";
                    }
                }
            }
            player.SetPlayerTextInfo($"{sbInfo}", isInfo: true);
            player.SetPlayerTextInfo($"{Role}");
            player.SetPlayerTextInfo("", isBottom: true);
        }
    }

    [HarmonyPatch(nameof(PlayerControl.MurderPlayer))]
    [HarmonyPostfix]
    public static void MurderPlayer_Postfix(/*PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target*/)
    {
        if (GameStates.IsInGame || GameStates.IsFreePlay && !GameStates.IsLobby && !GameStates.IsHideNSeek && HudManager.Instance.CrewmatesKilled.isActiveAndEnabled)
            HudManager.Instance?.NotifyOfDeath();
    }
}
