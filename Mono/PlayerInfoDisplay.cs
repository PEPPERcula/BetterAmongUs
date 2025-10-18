using AmongUs.Data;
using AmongUs.GameOptions;
using BetterAmongUs.Data;
using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using HarmonyLib;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Mono;

internal class PlayerInfoDisplay : MonoBehaviour
{
    protected PlayerControl? _player;
    protected TextMeshPro? _nameText;
    protected TextMeshPro? _infoText;
    protected TextMeshPro? _topText;
    protected TextMeshPro? _bottomText;

    internal void Init(PlayerControl player)
    {
        _player = player;

        var nameTextTransform = player.gameObject.transform.Find("Names/NameText_TMP");
        _nameText = nameTextTransform?.GetComponent<TextMeshPro>();

        _infoText = InstantiatePlayerInfoText("InfoText_Info_TMP", new Vector3(0f, 0.25f), nameTextTransform);
        _topText = InstantiatePlayerInfoText("InfoText_T_TMP", new Vector3(0f, 0.15f), nameTextTransform);
        _bottomText = InstantiatePlayerInfoText("InfoText_B_TMP", new Vector3(0f, -0.15f), nameTextTransform);
        _infoText.fontSize = 1.3f;
        _topText.fontSize = 1.3f;
        _bottomText.fontSize = 1.3f;
    }

    protected TextMeshPro InstantiatePlayerInfoText(string name, Vector3 positionOffset, Transform parent)
    {
        var newTextObject = Instantiate(_nameText, parent);
        newTextObject.name = name;
        newTextObject.transform.DestroyChildren();
        newTextObject.transform.position += positionOffset;

        var textMesh = newTextObject.GetComponent<TextMeshPro>();
        textMesh.text = string.Empty;
        newTextObject.gameObject.SetActive(true);

        return textMesh;
    }

    private void ResetText()
    {
        _infoText?.SetText(string.Empty);
        _topText?.SetText(string.Empty);
        _bottomText?.SetText(string.Empty);
    }

    protected virtual void LateUpdate()
    {
        UpdatePlayerInfo();
        UpdatePlayerHighlight();
        UpdateColorBlindTextPosition();
        _nameText.transform.parent.localPosition = new Vector3(0f, 0.8f, -0.5f);
    }

    private void UpdatePlayerInfo()
    {
        if (_player?.Data == null) return;

        var betterData = _player.BetterData();
        if (GameState.IsTOHEHostLobby) return;

        if (!_player.DataIsCollected())
        {
            _nameText.text = Translator.GetString("Player.Loading");
            return;
        }

        if (!BAUPlugin.LobbyPlayerInfo.Value && GameState.IsLobby)
        {
            ResetText();
            _player.RawSetName(_player.Data.PlayerName);
            return;
        }

        string newName = _player.Data.PlayerName;
        string hashPuid = Utils.GetHashPuid(_player);
        string platform = Utils.GetPlatformName(_player, useTag: true);

        string friendCode = ValidateFriendCode(out string friendCodeColor);

        if (DataManager.Settings.Gameplay.StreamerMode)
        {
            platform = Translator.GetString("Player.PlatformHidden");
        }

        var sbTag = new StringBuilder();
        var sbTagTop = new StringBuilder();
        var sbTagBottom = new StringBuilder();

        SetPlayerOutline(sbTag);

        if (GameState.IsInGame && GameState.IsLobby && !GameState.IsFreePlay)
        {
            SetLobbyInfo(ref newName, betterData, sbTag);
            sbTagTop.Append($"<color=#9e9e9e>{platform}</color>+++")
                    .Append($"<color=#ffd829>Lv: {_player.Data.PlayerLevel + 1}</color>+++");

            sbTagBottom.Append($"<color={friendCodeColor}>{friendCode}</color>+++");
        }
        else if ((GameState.IsInGame || GameState.IsFreePlay) && !GameState.IsHideNSeek)
        {
            SetInGameInfo(sbTagTop);
        }

        if (!_player.IsInShapeshift())
        {
            _player.RawSetName(newName);
        }
        else
        {
            var targetData = Utils.PlayerDataFromPlayerId(_player.shapeshiftTargetPlayerId);
            if (targetData != null) _player.RawSetName(targetData.BetterData().RealName);
        }

        _topText?.SetText(FormatInfo(sbTagTop));
        _bottomText?.SetText(FormatInfo(sbTagBottom));
        _infoText?.SetText(FormatInfo(sbTag));
    }

    private string ValidateFriendCode(out string color)
    {
        color = "#FFFFFF";
        if (_player?.Data == null) return string.Empty;

        void TryKick()
        {
            if (GameState.IsHost && BetterGameSettings.InvalidFriendCode.GetBool())
            {
                string kickMessage = string.Format(Translator.GetString("AntiCheat.KickMessage"),
                    Translator.GetString("AntiCheat.ByAntiCheat"),
                    Translator.GetString("AntiCheat.Reason.InvalidFriendCode"));
                _player.Kick(true, kickMessage, true);
            }
        }

        string friendCode = _player.Data.FriendCode;
        string pattern = @"^[a-zA-Z0-9#]+$";
        string hashtagPattern = @"^#[0-9]{4}$";

        bool isValidFriendCode = !string.IsNullOrEmpty(friendCode) &&
                               Regex.IsMatch(friendCode, pattern) &&
                               Regex.IsMatch(friendCode, hashtagPattern) &&
                               !friendCode.Contains(' ') &&
                               Regex.Replace(friendCode, hashtagPattern, string.Empty).Length is > 10 or < 5;

        color = isValidFriendCode ? "#ff0000" : "#00f7ff";

        if (string.IsNullOrEmpty(friendCode))
        {
            friendCode = Translator.GetString("Player.NoFriendCode");
            color = "#ff0000";
            TryKick();
        }
        else if (!isValidFriendCode)
        {
            TryKick();
        }

        if (DataManager.Settings.Gameplay.StreamerMode)
        {
            friendCode = new string('*', friendCode.Length);
        }

        return friendCode.Trim();
    }

    private void SetPlayerOutline(StringBuilder sbTag)
    {
        string hashPuid = Utils.GetHashPuid(_player);
        string friendCode = _player.Data.FriendCode;

        var color = _player.cosmetics.currentBodySprite.BodySprite.material.GetColor("_OutlineColor");

        if (BetterDataManager.BetterDataFile.SickoData.Any(info => info.CheckPlayerData(_player.Data)))
        {
            sbTag.Append($"<color=#00f583>{Translator.GetString("Player.SickoUser")}</color>+++");
            _player.SetOutlineByHex(true, "#00f583");
        }
        else if (BetterDataManager.BetterDataFile.AUMData.Any(info => info.CheckPlayerData(_player.Data)))
        {
            sbTag.Append($"<color=#4f0000>{Translator.GetString("Player.AUMUser")}</color>+++");
            _player.SetOutlineByHex(true, "#4f0000");
        }
        else if (BetterDataManager.BetterDataFile.KNData.Any(info => info.CheckPlayerData(_player.Data)))
        {
            sbTag.Append($"<color=#8731e7>{Translator.GetString("Player.KNUser")}</color>+++");
            _player.SetOutlineByHex(true, "#8731e7");
        }
        else if (BetterDataManager.BetterDataFile.CheatData.Any(info => info.CheckPlayerData(_player.Data)))
        {
            sbTag.Append($"<color=#fc0000>{Translator.GetString("Player.KnownCheater")}</color>+++");
            _player.SetOutlineByHex(true, "#fc0000");
        }
        else if (color == Utils.HexToColor32("#00f583") || color == Utils.HexToColor32("#4f0000") ||
                 color == Utils.HexToColor32("#fc0000") || color == Utils.HexToColor32("#8731e7"))
        {
            _player.SetOutline(false, null);
        }
    }

    private void SetLobbyInfo(ref string newName, ExtendedPlayerInfo betterData, StringBuilder sbTag)
    {
        if (_player.IsHost() && BAUPlugin.LobbyPlayerInfo.Value)
            newName = _player.GetPlayerNameAndColor();

        if ((_player.IsLocalPlayer() || betterData.IsBetterUser) && !GameState.IsInGamePlay)
        {
            string verificationSymbol = betterData.IsVerifiedBetterUser || _player.IsLocalPlayer() ? "✓ " : "";
            sbTag.AppendFormat("<color=#0dff00>{1}{0}</color>+++",
                Translator.GetString("Player.BetterUser"), verificationSymbol);
        }
        sbTag.Append($"<color=#b554ff>ID: {_player.PlayerId}</color>+++");
    }

    private void SetInGameInfo(StringBuilder sbTagTop)
    {
        if (_player.IsImpostorTeammate() || _player.IsLocalPlayer() ||
            !PlayerControl.LocalPlayer.IsAlive() && !PlayerControl.LocalPlayer.Is(RoleTypes.GuardianAngel))
        {
            string roleInfo = $"<color={_player.GetTeamHexColor()}>{_player.GetRoleName()}</color>";

            if (!_player.IsImpostorTeam() && _player.myTasks.Count > 0)
            {
                int completedTasks = _player.Data.Tasks.ToArray().Count(task => task.Complete);
                roleInfo += $" <color=#cbcbcb>({completedTasks}/{_player.Data.Tasks.Count})</color>";
            }

            sbTagTop.Append(roleInfo + "+++");
        }
    }

    private void UpdatePlayerHighlight()
    {
        SetPlayerOutline(new StringBuilder());
    }

    private void UpdateColorBlindTextPosition()
    {
        var text = _player.cosmetics.colorBlindText;
        if (!text.enabled) return;
        if (!_player.onLadder && !_player.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
        {
            text.transform.localPosition = new Vector3(0f, -1.3f, 0.4999f);
        }
        else
        {
            text.transform.localPosition = new Vector3(0f, -1.5f, 0.4999f);
        }
    }

    private static string FormatInfo(StringBuilder source)
    {
        if (source.Length == 0) return string.Empty;

        var sb = new StringBuilder();
        foreach (var part in source.ToString().Split("+++"))
        {
            if (!string.IsNullOrEmpty(Utils.RemoveHtmlText(part)))
            {
                sb.Append(part).Append(" - ");
            }
        }
        return sb.ToString().TrimEnd(" - ".ToCharArray());
    }
}