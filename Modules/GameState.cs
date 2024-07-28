using AmongUs.GameOptions;
using UnityEngine;

namespace BetterAmongUs;

// Code from: https://github.com/0xDrMoe/TownofHost-Enhanced

public static class GameStates
{
    /**********Check Game Status***********/
    public static bool IsDev => Main.DevUser.Contains(Utils.GetHashPuid(EOSManager.Instance.ProductUserId));
    public static bool InGame => Main.AllPlayerControls.Any();
    public static bool IsNormalGame => GameOptionsManager.Instance.CurrentGameOptions.GameMode is GameModes.Normal or GameModes.NormalFools;
    public static bool IsHideNSeek => GameOptionsManager.Instance.CurrentGameOptions.GameMode is GameModes.HideNSeek or GameModes.SeekFools;
    public static bool SkeldIsActive => (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == MapNames.Skeld;
    public static bool MiraHQIsActive => (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == MapNames.Mira;
    public static bool PolusIsActive => (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == MapNames.Polus;
    public static bool DleksIsActive => (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == MapNames.Dleks;
    public static bool AirshipIsActive => (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == MapNames.Airship;
    public static bool FungleIsActive => (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == MapNames.Fungle;
    public static bool IsInGame => InGame;
    public static bool IsLobby => AmongUsClient.Instance?.GameState == InnerNet.InnerNetClient.GameStates.Joined;
    public static bool IsInIntro => IntroCutscene.Instance != null;
    public static bool IsInGamePlay => (InGame && !IsLobby && !IsInIntro) || IsFreePlay;
    public static bool IsEnded => AmongUsClient.Instance?.GameState == InnerNet.InnerNetClient.GameStates.Ended;
    public static bool IsNotJoined => AmongUsClient.Instance?.GameState == InnerNet.InnerNetClient.GameStates.NotJoined;
    public static bool IsOnlineGame => AmongUsClient.Instance?.NetworkMode == NetworkModes.OnlineGame;
    public static bool IsVanillaServer
    {
        get
        {
            if (!IsOnlineGame) return false;

            string region = ServerManager.Instance.CurrentRegion.Name;
            return (region == "North America" || region == "Europe" || region == "Asia");
        }
    }
    public static bool IsLocalGame => AmongUsClient.Instance?.NetworkMode == NetworkModes.LocalGame;
    public static bool IsFreePlay => AmongUsClient.Instance?.NetworkMode == NetworkModes.FreePlay;
    public static bool IsInTask => InGame && MeetingHud.Instance == null;
    public static bool IsMeeting => InGame && MeetingHud.Instance != null;
    public static bool IsVoting => IsMeeting && MeetingHud.Instance?.state is MeetingHud.VoteStates.Voted or MeetingHud.VoteStates.NotVoted;
    public static bool IsProceeding => IsMeeting && MeetingHud.Instance?.state is MeetingHud.VoteStates.Proceeding;
    public static bool IsExilling => ExileController.Instance != null && !(AirshipIsActive && Minigame.Instance != null && Minigame.Instance.isActiveAndEnabled);
    public static bool IsCountDown => GameStartManager.InstanceExists && GameStartManager.Instance.startState == GameStartManager.StartingStates.Countdown;
    /**********TOP ZOOM.cs***********/
    public static bool IsShip => ShipStatus.Instance != null;
    public static bool IsHost => AmongUsClient.Instance.AmHost;
    public static bool IsCanMove => PlayerControl.LocalPlayer?.CanMove is true;
    public static bool IsDead => PlayerControl.LocalPlayer?.Data?.IsDead is true;
}
