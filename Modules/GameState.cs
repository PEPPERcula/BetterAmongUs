using AmongUs.GameOptions;
using BetterAmongUs.Helpers;

namespace BetterAmongUs.Modules;

internal static class GameState
{
    /**********Check Game Status***********/
    internal static bool IsDev => Main.MyData.IsDev();
    internal static bool InGame => Main.AllPlayerControls.Any();
    internal static bool IsNormalGame => GameOptionsManager.Instance.CurrentGameOptions.GameMode is GameModes.Normal or GameModes.NormalFools;
    internal static bool IsHideNSeek => GameOptionsManager.Instance != null && GameOptionsManager.Instance.CurrentGameOptions.GameMode is GameModes.HideNSeek or GameModes.SeekFools;
    internal static bool SkeldIsActive => (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == MapNames.Skeld;
    internal static bool MiraHQIsActive => (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == MapNames.MiraHQ;
    internal static bool PolusIsActive => (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == MapNames.Polus;
    internal static bool DleksIsActive => (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == MapNames.Dleks;
    internal static bool AirshipIsActive => (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == MapNames.Airship;
    internal static bool FungleIsActive => (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == MapNames.Fungle;
    internal static byte GetActiveMapId => GameOptionsManager.Instance.CurrentGameOptions.MapId;
    internal static bool IsSystemActive(SystemTypes type)
    {
        if (IsHideNSeek || !ShipStatus.Instance.Systems.TryGetValue(type, out var system))
        {
            return false;
        }

        int mapId = GetActiveMapId;

        return type switch
        {
            SystemTypes.Electrical when mapId != 5 => !system.Cast<SwitchSystem>()?.IsActive == false,
            SystemTypes.Reactor when mapId != 2 => system.Cast<ReactorSystemType>()?.IsActive ?? false,
            SystemTypes.Laboratory when mapId == 2 => system.Cast<ReactorSystemType>()?.IsActive ?? false,
            SystemTypes.LifeSupp when mapId is 0 or 3 => system.Cast<LifeSuppSystemType>()?.IsActive ?? false,
            SystemTypes.HeliSabotage when mapId == 4 => system.Cast<HeliSabotageSystem>()?.IsActive ?? false,
            SystemTypes.Comms when mapId is 1 or 5 => system.Cast<HqHudSystemType>()?.IsActive ?? false,
            SystemTypes.Comms => system.Cast<HudOverrideSystemType>()?.IsActive ?? false,
            SystemTypes.MushroomMixupSabotage when mapId == 5 => system.Cast<MushroomMixupSabotageSystem>()?.IsActive ?? false,
            _ => false
        };
    }
    internal static bool IsCriticalSabotageActive()
    {
        var deathSabotages = new[]
        {
        SystemTypes.Reactor,
        SystemTypes.Laboratory,
        SystemTypes.LifeSupp,
        SystemTypes.HeliSabotage,
    };

        return deathSabotages.Any(IsSystemActive);
    }
    internal static bool IsNoneCriticalSabotageActive()
    {
        var noneDeathSabotages = new[]
        {
        SystemTypes.Electrical,
        SystemTypes.Comms,
        SystemTypes.MushroomMixupSabotage
    };

        return noneDeathSabotages.Any(IsSystemActive);
    }
    internal static bool IsAnySabotageActive()
    {
        var allSabotages = new[]
        {
        SystemTypes.Electrical,
        SystemTypes.Reactor,
        SystemTypes.Laboratory,
        SystemTypes.LifeSupp,
        SystemTypes.HeliSabotage,
        SystemTypes.Comms,
        SystemTypes.MushroomMixupSabotage
    };

        return allSabotages.Any(IsSystemActive);
    }
    internal static bool IsInGame => InGame;
    internal static bool IsLobby => AmongUsClient.Instance?.GameState == InnerNet.InnerNetClient.GameStates.Joined;
    internal static bool IsInIntro => IntroCutscene.Instance != null;
    internal static bool IsInGamePlay => InGame && IsShip && !IsLobby && !IsInIntro || IsFreePlay;
    internal static bool IsEnded => AmongUsClient.Instance?.GameState == InnerNet.InnerNetClient.GameStates.Ended;
    internal static bool IsNotJoined => AmongUsClient.Instance?.GameState == InnerNet.InnerNetClient.GameStates.NotJoined;
    internal static bool IsOnlineGame => AmongUsClient.Instance?.NetworkMode == NetworkModes.OnlineGame;
    internal static bool IsVanillaServer
    {
        get
        {
            if (!IsOnlineGame) return false;

            string region = ServerManager.Instance.CurrentRegion.Name;
            return region == "North America" || region == "Europe" || region == "Asia";
        }
    }
    internal static bool IsLocalGame => AmongUsClient.Instance?.NetworkMode == NetworkModes.LocalGame;
    internal static bool IsFreePlay => AmongUsClient.Instance?.NetworkMode == NetworkModes.FreePlay;
    internal static bool IsInTask => InGame && MeetingHud.Instance == null;
    internal static bool IsMeeting => InGame && MeetingHud.Instance != null;
    internal static bool IsVoting => IsMeeting && MeetingHud.Instance?.state is MeetingHud.VoteStates.Voted or MeetingHud.VoteStates.NotVoted;
    internal static bool IsProceeding => IsMeeting && MeetingHud.Instance?.state is MeetingHud.VoteStates.Proceeding;
    internal static bool IsExilling => ExileController.Instance != null && !(AirshipIsActive && Minigame.Instance != null && Minigame.Instance.isActiveAndEnabled);
    internal static bool IsCountDown => GameStartManager.InstanceExists && GameStartManager.Instance.startState == GameStartManager.StartingStates.Countdown;
    internal static bool IsShip => ShipStatus.Instance != null;
    internal static bool IsHost => AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost;
    internal static bool IsPrivateOnlyLobby => (Main.PrivateOnlyLobby.Value || AmongUsClient.Instance.AmLocalHost) && IsHost;
    internal static bool IsBetterHostLobby => PlayerControl.LocalPlayer.IsHost() || Main.AllPlayerControls.Any(pc => pc.IsHost() && pc.BetterData().IsBetterUser && pc.BetterData().IsVerifiedBetterUser);
    internal static bool IsTOHEHostLobby => Main.AllPlayerControls.Any(pc => pc.IsHost() && pc.BetterData().IsTOHEHost);
    internal static bool IsCanMove => PlayerControl.LocalPlayer?.CanMove is true;
    internal static bool IsDead => PlayerControl.LocalPlayer?.Data?.IsDead is true;
}
