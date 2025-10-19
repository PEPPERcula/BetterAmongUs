using AmongUs.GameOptions;
using BetterAmongUs.Data;
using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using System.Text;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Mono;

internal class MeetingInfoDisplay : PlayerInfoDisplay
{
    private PlayerVoteArea? _pva;
    private Vector3 _namePos;
    private Vector3 _infoPos;
    private Vector3 _TopPos;

    internal void Init(PlayerControl? player, PlayerVoteArea pva)
    {
        _player = player;
        _pva = pva;

        _nameText = pva.NameText;
        _infoText = InstantiatePlayerInfoText("InfoText_Info_TMP", new Vector3(0f, 0.28f), pva.transform);
        _topText = InstantiatePlayerInfoText("InfoText_T_TMP", new Vector3(0f, 0.15f), pva.transform);
        _infoText.fontSize = 1.3f;
        _topText.fontSize = 1.3f;
        _namePos = _nameText.transform.localPosition - new Vector3(0f, 0.02f, 0f);
        _infoPos = _infoText.transform.localPosition;
        _TopPos = _topText.transform.localPosition;

        var PlayerLevel = pva.transform.Find("PlayerLevel");
        PlayerLevel.localPosition = new Vector3(PlayerLevel.localPosition.x, PlayerLevel.localPosition.y, -2f);
        var LevelDisplay = Instantiate(PlayerLevel, pva.transform);
        LevelDisplay.transform.SetSiblingIndex(pva.transform.Find("PlayerLevel").GetSiblingIndex() + 1);
        LevelDisplay.gameObject.name = "PlayerId";
        LevelDisplay.GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 1f, 1f);
        var IdLabel = LevelDisplay.transform.Find("LevelLabel");
        var IdNumber = LevelDisplay.transform.Find("LevelNumber");
        IdLabel.gameObject.DestroyTextTranslators();
        IdLabel.GetComponent<TextMeshPro>().text = "ID";
        IdNumber.GetComponent<TextMeshPro>().text = pva.TargetPlayerId.ToString();
        IdLabel.name = "IdLabel";
        IdNumber.name = "IdNumber";
        PlayerLevel.transform.position += new Vector3(0.23f, 0f);
    }

    protected override void LateUpdate()
    {
        if (_player != null)
        {
            UpdateInfo();
        }
        else
        {
            UpdateDisconnect();
        }

        if (_pva != null)
        {
            if (_infoText.text != string.Empty && _topText.text != string.Empty)
            {
                _nameText.transform.localPosition = _namePos + new Vector3(0f, -0.1f, 0f);
                _infoText.transform.localPosition = _infoPos + new Vector3(0f, -0.1f, 0f);
                _topText.transform.localPosition = _TopPos + new Vector3(0f, -0.1f, 0f);
            }
            else if (_infoText.text != string.Empty || _topText.text != string.Empty)
            {
                _nameText.transform.localPosition = _namePos;
                _infoText.transform.localPosition = _TopPos;
                _topText.transform.localPosition = _TopPos;
            }
            else
            {
                _nameText.transform.localPosition = _namePos;
                _infoText.transform.localPosition = _infoPos;
                _topText.transform.localPosition = _TopPos;
            }

            _pva.ColorBlindName.transform.localPosition = new Vector3(-0.91f, -0.19f, -0.05f);
        }
    }

    private void UpdateInfo()
    {
        var target = Utils.PlayerFromPlayerId(_pva.TargetPlayerId);
        if (target == null) return;

        string hashPuid = Utils.GetHashPuid(target);
        string friendCode = target.Data.FriendCode;

        StringBuilder sbTag = new();
        StringBuilder sbInfo = new();

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
            textPos = new Vector3(_pva.NameText.transform.localPosition.x, -0.045f);
        else if (Role != "" || !string.IsNullOrEmpty(sbInfo.ToString()))
            textPos = new Vector3(_pva.NameText.transform.localPosition.x, 0.015f);
        else
            textPos = new Vector3(_pva.NameText.transform.localPosition.x, 0.015f);

        _pva.NameText.transform.localPosition = textPos;

        _infoText.SetText($"{sbInfo}");
        _topText.SetText($"{Role}");
    }

    private void UpdateDisconnect()
    {
        string DisconnectText;
        var playerData = GameData.Instance.GetPlayerById(_pva.TargetPlayerId);
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

        _infoText?.SetText($"<color=#6b6b6b>{DisconnectText}</color>");
        _topText?.SetText("");
        _pva.transform.Find("votePlayerBase").gameObject.SetActive(false);
        _pva.transform.Find("deadX_border").gameObject.SetActive(false);
        _pva.ClearForResults();
        _pva.SetDisabled();
    }
}
