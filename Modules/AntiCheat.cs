using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using InnerNet;
using UnityEngine;

namespace BetterAmongUs;

class AntiCheat
{
    public static Dictionary<string, string> PlayerData = []; // HashPuid, FriendCode
    public static Dictionary<string, string> SickoData = []; // HashPuid, FriendCode
    public static Dictionary<string, string> AUMData = []; // HashPuid, FriendCode
    private static bool IsEnabled = true;

    public static void Update()
    {
        if (GameStates.IsInGamePlay)
        {
            foreach (var kvp in ExtendedPlayerInfo.TimeSinceKill)
            {
                ExtendedPlayerInfo.TimeSinceKill[kvp.Key] += Time.deltaTime;
            }
        }
        else
        {
            if (ExtendedPlayerInfo.TimeSinceKill.Any())
            {
                ExtendedPlayerInfo.TimeSinceKill.Clear();
            }

            if (ExtendedPlayerInfo.TimesCalledMeeting.Any())
            {
                ExtendedPlayerInfo.TimesCalledMeeting.Clear();
            }

            if (ExtendedPlayerInfo.HasNoisemakerNotify.Any())
            {
                ExtendedPlayerInfo.HasNoisemakerNotify.Clear();
            }
        }

        if (GameStates.IsHost && GameStates.IsInGame)
        {
            RPC.SyncAllNames(isBetterHost: false);

            foreach (var player in Main.AllPlayerControls)
            {
                var hashPuid = Utils.GetHashPuid(player);

                if (SickoData.ContainsKey(hashPuid))
                {
                    player.RpcSetName($"<color=#ffea00>{player.Data.PlayerName}</color> Has been banned by <color=#4f92ff>Anti-Cheat</color>, Reason: Known Sicko Menu User<size=0%>");
                    AmongUsClient.Instance.KickPlayer(player.GetClientId(), true);
                }
                else if (AUMData.ContainsKey(hashPuid))
                {
                    player.RpcSetName($"<color=#ffea00>{player.Data.PlayerName}</color> Has been banned by <color=#4f92ff>Anti-Cheat</color>, Reason: Known AUM User<size=0%>");
                    AmongUsClient.Instance.KickPlayer(player.GetClientId(), true);
                }
                else if (PlayerData.ContainsKey(hashPuid))
                {
                    player.RpcSetName($"<color=#ffea00>{player.Data.PlayerName}</color> Has been banned by <color=#4f92ff>Anti-Cheat</color>, Reason: Known Cheater<size=0%>");
                    AmongUsClient.Instance.KickPlayer(player.GetClientId(), true);
                }
            }
        }
    }

    public static void PauseAntiCheat()
    {
        float time = 2f;
        if (IsEnabled)
        {
            IsEnabled = false;

            _ = new LateTask(() =>
            {
                IsEnabled = true;
            }, time, "PauseAntiCheat");
        }
    }

    [HarmonyPatch(typeof(PlatformSpecificData))]
    class PlatformSpecificDataPatch
    {
        [HarmonyPatch(nameof(PlatformSpecificData.Deserialize))]
        [HarmonyPostfix]
        public static void Deserialize_Postfix(PlatformSpecificData __instance)
        {
            _ = new LateTask(() =>
                {
                var player = Main.AllPlayerControls.FirstOrDefault(pc => pc.GetClient().PlatformData == __instance);

                if (player != null)
                {
                    if (__instance.Platform is Platforms.StandaloneWin10 or Platforms.Xbox)
                    {
                        if (__instance.XboxPlatformId == 0)
                        {
                            BetterNotificationManager.NotifyCheat(player, $"Platform Spoofer", newText: "Has been detected with a cheat");
                        }
                    }

                    if (__instance.Platform is Platforms.Playstation)
                    {
                        if (__instance.PsnPlatformId == 0)
                        {
                            BetterNotificationManager.NotifyCheat(player, $"Platform Spoofer", newText: "Has been detected with a cheat");
                        }
                    }
                }
            }, 1.5f, shoudLog: false);
        }
    }


    // Handle RPC before anti cheat detection
    public static void HandleRPCBeforeCheck(PlayerControl player, byte callId, MessageReader Oldreader)
    {
        MessageReader reader = MessageReader.Get(Oldreader);

        if (player == PlayerControl.LocalPlayer || PlayerControl.LocalPlayer == null || player == null || !IsEnabled) return;

        if (callId is unchecked((byte)CustomRPC.Sicko) && Main.AntiCheat.Value)
        {
            if (reader.BytesRemaining == 0)
            {
                var flag = SickoData.ContainsKey(Utils.GetHashPuid(player));

                if (reader.BytesRemaining == 0 && !flag)
                {
                    AmongUsClient.Instance.ReportPlayer(player.GetClientId(), ReportReasons.Cheating_Hacking);
                    SickoData[Utils.GetHashPuid(player)] = player.FriendCode;
                    BetterDataManager.SaveCheatData(Utils.GetHashPuid(player), player.Data.FriendCode, player.Data.PlayerName, "sickoData", "Sicko Menu RPC");
                    BetterNotificationManager.NotifyCheat(player, $"Sicko Menu", newText: "Has been detected with a cheat client");
                }
            }

            return;
        }

        if (callId is unchecked((byte)CustomRPC.AUM) && Main.AntiCheat.Value)
        {
            try
            {
                var aumid = reader.ReadByte();

                if (aumid == player.PlayerId)
                {
                    var flag = AUMData.ContainsKey(Utils.GetHashPuid(player));

                    if (!flag)
                    {
                        AmongUsClient.Instance.ReportPlayer(player.GetClientId(), ReportReasons.Cheating_Hacking);
                        AUMData[Utils.GetHashPuid(player)] = player.FriendCode;
                        BetterDataManager.SaveCheatData(Utils.GetHashPuid(player), player.Data.FriendCode, player.Data.PlayerName, "aumData", "AUM RPC");
                        BetterNotificationManager.NotifyCheat(player, $"AUM", newText: "Has been detected with a cheat client");
                    }
                }
            }
            catch { }

            return;
        }

        if (callId is unchecked((byte)CustomRPC.AUMChat))
        {
            try
            {
                var nameString = reader.ReadString();
                var msgString = reader.ReadString();
                var colorId = reader.ReadInt32();

                Utils.AddChatPrivate($"{msgString}", overrideName: $"<b><color=#870000>AUM Chat</color> - <color={Utils.Color32ToHex(Palette.PlayerColors[colorId])}>{nameString}</color></b>");

                PlayerControl AUMPlayer = Main.AllPlayerControls.First(pc => pc.Data.PlayerName == nameString && pc.CurrentOutfit.ColorId == colorId);

                if (AUMPlayer == null || !Main.AntiCheat.Value) return;

                var flag = string.IsNullOrEmpty(nameString) && string.IsNullOrEmpty(msgString);
                var flag2 = AUMData.ContainsKey(Utils.GetHashPuid(AUMPlayer));

                if (!flag && !flag2)
                {
                    AmongUsClient.Instance.ReportPlayer(player.GetClientId(), ReportReasons.Cheating_Hacking);
                    AUMData[Utils.GetHashPuid(AUMPlayer)] = AUMPlayer.FriendCode;
                    BetterDataManager.SaveCheatData(Utils.GetHashPuid(player), player.Data.FriendCode, player.Data.PlayerName, "aumData", "AUM Chat RPC");
                    BetterNotificationManager.NotifyCheat(AUMPlayer, $"AUM", newText: "Has been detected with a cheat client");
                }
            }
            catch { }

            return;
        }
    }

    // Check and notify for invalid rpcs
    public static void CheckRPC(PlayerControl player, byte callId, MessageReader Oldreader)
    {
        try
        {
            MessageReader reader = MessageReader.Get(Oldreader);

            if (PlayerControl.LocalPlayer == null || player == null || player == PlayerControl.LocalPlayer || player.GetIsBetterHost() || reader == null || !IsEnabled || !Main.AntiCheat.Value) return;

            RoleTypes? Role = player?.Data?.RoleType;
            Role ??= RoleTypes.Crewmate;
            bool IsImpostor = player.IsImpostorTeam();
            bool IsCrewmate = !player.IsImpostorTeam();
            string hashPuid = Utils.GetHashPuid(player);

            if (callId is (byte)RpcCalls.SendChat or (byte)RpcCalls.SendQuickChat)
            {
                if (player.IsAlive() && GameStates.IsInGamePlay && !GameStates.IsMeeting && !GameStates.IsExilling)
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");

                return;
            }

            if (callId is (byte)RpcCalls.EnterVent or (byte)RpcCalls.ExitVent)
            {
                if ((IsCrewmate && Role != RoleTypes.Engineer) || player.Data.IsDead)
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");

                return;
            }

            if (callId is (byte)RpcCalls.ProtectPlayer)
            {
                if (Role is not RoleTypes.GuardianAngel)
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");

                return;
            }

            if (callId is (byte)RpcCalls.MurderPlayer)
            {
                if (reader.BytesRemaining > 0)
                {
                    PlayerControl target = reader.ReadNetObject<PlayerControl>();

                    if (target != null)
                    {
                        if (IsCrewmate || !player.IsAlive() || player.IsInVanish() || !target.IsAlive() || target.IsImpostorTeam()
                           || ExtendedPlayerInfo.TimeSinceKill.TryGetValue(player, out var value) && value < (float)GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown)
                        {
                            BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                        }
                    }
                }

                return;
            }

            if (callId is (byte)RpcCalls.SetLevel)
            {
                if (reader.BytesRemaining > 0)
                {
                    uint level = reader.ReadPackedUInt32();
                    if (level >= 200)
                    {
                        BetterNotificationManager.NotifyCheat(player, $"Invalid Level: {level}");
                    }
                }

                return;
            }

            if (IsCrewmate)
            {
                if (GameStates.IsInGamePlay)
                {
                    if (callId is (byte)RpcCalls.CloseDoorsOfType)
                    {
                        byte Type = reader.ReadByte();
                        BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)} - {Enum.GetName((SystemTypes)callId)}");

                        return;
                    }
                    if (callId is (byte)RpcCalls.UpdateSystem)
                    {
                        byte Type = reader.ReadByte();
                        if (Type is (byte)SystemTypes.Reactor
                            or (byte)SystemTypes.Laboratory
                            or (byte)SystemTypes.Comms
                            or (byte)SystemTypes.LifeSupp
                            or (byte)SystemTypes.MushroomMixupSabotage
                            or (byte)SystemTypes.HeliSabotage)
                        {
                            BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)} - {Enum.GetName((SystemTypes)callId)}");
                        }

                        return;
                    }
                }
            }

            if (IsImpostor)
            {
                if (callId is (byte)RpcCalls.CompleteTask or (byte)RpcCalls.BootFromVent)
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");

                    return;
                }
            }

            if (callId is (byte)RpcCalls.Pet or (byte)RpcCalls.CancelPet)
            {
                if (player?.CurrentOutfit?.PetId == "pet_EmptyPet")
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                }

                return;
            }
        }
        catch (Exception ex) 
        {
            Logger.Error(ex.ToString());
        }
    }

    // Check notify and cancel out request for invalid rpcs
    public static bool CheckCancelRPC(PlayerControl player, byte callId, MessageReader Oldreader)
    {
        try
        {
            MessageReader reader = MessageReader.Get(Oldreader);

            if (PlayerControl.LocalPlayer == null || player == null || player == PlayerControl.LocalPlayer || player.GetIsBetterHost() || reader == null || !IsEnabled || !Main.AntiCheat.Value) return true;

            RoleTypes Role = player.Data.RoleType;
            bool IsImpostor = player.IsImpostorTeam();
            bool IsCrewmate = !player.IsImpostorTeam();

            if (TrustedRPCs(callId) != true)
            {
                BetterNotificationManager.NotifyCheat(player, $"Untrusted RPC received: {callId}");
                return false;
            }

            if (callId is (byte)RpcCalls.StartMeeting)
            {
                if (GameStates.IsMeeting || GameStates.IsHideNSeek || !player.IsAlive() || player.IsInVent() || player.shapeshifting
                    || player.inMovingPlat || player.onLadder || player.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                    if (GameStates.IsHost)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }

                var deadPlayerInfo = GameData.Instance.GetPlayerById(reader.ReadByte());
                bool isBodyReport = deadPlayerInfo != null;

                if (isBodyReport)
                {
                    if (!UnityEngine.Object.FindAnyObjectByType<DeadBody>() || !deadPlayerInfo.IsDead || deadPlayerInfo == player.Data)
                    {
                        BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                        if (GameStates.IsHost)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (ExtendedPlayerInfo.TimesCalledMeeting.TryGetValue(player, out var value) && value >= GameOptionsManager.Instance.currentNormalGameOptions.NumEmergencyMeetings)
                    {
                        BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                        if (GameStates.IsHost)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }

            if (callId is (byte)RpcCalls.Shapeshift)
            {
                var flag = reader.ReadBoolean();

                if (Role is not RoleTypes.Shapeshifter || !player.IsAlive())
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                    return false;
                }

                if (!flag && !player.IsInVent())
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                    return false;
                }
            }

            if (callId is (byte)RpcCalls.StartVanish or (byte)RpcCalls.StartAppear)
            {
                if (Role is not RoleTypes.Phantom || player.Data.IsDead)
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                    return false;
                }
            }

            if (!player.IsHost())
            {
                if (callId is (byte)RpcCalls.SetTasks
                or (byte)RpcCalls.ExtendLobbyTimer
                or (byte)RpcCalls.CloseMeeting)
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Host RPC: {Enum.GetName((RpcCalls)callId)}");
                    return false;
                }
            }

            if (player.DataIsCollected() == true)
            {
                if (callId is (byte)RpcCalls.SetName or (byte)RpcCalls.CheckName or (byte)RpcCalls.SetLevel)
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Set RPC: {Enum.GetName((RpcCalls)callId)}");
                    return false;
                }
            }

            if (GameStates.IsInGamePlay)
            {
                if (callId is (byte)RpcCalls.SetColor
                    or (byte)RpcCalls.SetHat
                    or (byte)RpcCalls.SetSkin
                    or (byte)RpcCalls.SetVisor
                    or (byte)RpcCalls.SetPet
                    or (byte)RpcCalls.SetNamePlate)
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Set RPC: {Enum.GetName((RpcCalls)callId)}");
                    return false;
                }
            }

            if (GameStates.IsInGame && GameStates.IsLobby)
            {
                if (callId is (byte)RpcCalls.StartMeeting
                    or (byte)RpcCalls.ReportDeadBody
                    or (byte)RpcCalls.CloseMeeting
                    or (byte)RpcCalls.Exiled
                    or (byte)RpcCalls.CastVote
                    or (byte)RpcCalls.ClearVote
                    or (byte)RpcCalls.SetRole
                    or (byte)RpcCalls.ClimbLadder
                    or (byte)RpcCalls.UsePlatform
                    or (byte)RpcCalls.UseZipline
                    or (byte)RpcCalls.CompleteTask
                    or (byte)RpcCalls.BootFromVent
                    or (byte)RpcCalls.EnterVent
                    or (byte)RpcCalls.ExitVent
                    or (byte)RpcCalls.CloseDoorsOfType
                    or (byte)RpcCalls.MurderPlayer
                    or (byte)RpcCalls.CheckMurder
                    or (byte)RpcCalls.Shapeshift
                    or (byte)RpcCalls.RejectShapeshift
                    or (byte)RpcCalls.CheckShapeshift
                    or (byte)RpcCalls.CheckProtect
                    or (byte)RpcCalls.ProtectPlayer
                    or (byte)RpcCalls.StartAppear
                    or (byte)RpcCalls.StartVanish
                    or (byte)RpcCalls.CheckAppear
                    or (byte)RpcCalls.CheckVanish)
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Lobby RPC: {Enum.GetName((RpcCalls)callId)}");
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex.ToString());
            return true;
        }
    }

    // Check if RPC is known
    private static bool TrustedRPCs(int RPCId)
    {
        foreach (RpcCalls rpc in Enum.GetValues(typeof(RpcCalls)))
            if ((byte)rpc == RPCId || unchecked((byte)rpc) == RPCId || unchecked((byte)(short)rpc) == RPCId)
                return true;
        foreach (CustomRPC rpc in Enum.GetValues(typeof(CustomRPC)))
            if ((byte)rpc == RPCId || unchecked((byte)rpc) == RPCId || unchecked((byte)(short)rpc) == RPCId)
                return true;

        return false;
    }
}
