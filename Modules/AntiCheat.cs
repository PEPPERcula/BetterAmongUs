using AmongUs.GameOptions;
using Hazel;
using InnerNet;

namespace BetterAmongUs;

class AntiCheat
{
    public static Dictionary<string, string> PlayerData = []; // HashPuid, FriendCode
    public static Dictionary<string, string> SickoData = []; // HashPuid, FriendCode
    public static Dictionary<string, string> AUMData = []; // HashPuid, FriendCode
    private static bool IsEnabled = true;

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

    // Handle RPC before anti cheat detection
    public static void HandleRPCBeforeCheck(PlayerControl player, byte callId, MessageReader reader)
    {
        if (player == PlayerControl.LocalPlayer || PlayerControl.LocalPlayer == null || player == null || !IsEnabled) return;

        if (callId is unchecked((byte)CustomRPC.Sicko) && Main.AntiCheat.Value)
        {
            if (reader.BytesRemaining == 0)
            {
                var flag = SickoData.ContainsKey(Utils.GetHashPuid(player));

                if (reader.BytesRemaining == 0 && !flag)
                {
                    AmongUsClient.Instance.ReportPlayer(player.GetClientId(), ReportReasons.Cheating_Hacking);
                    BAUNotificationManager.NotifyCheat(player, $"Sicko Menu", newText: "Has been detected with a cheat client");
                    PlayerData[Utils.GetHashPuid(player)] = player.FriendCode;
                    SickoData[Utils.GetHashPuid(player)] = player.FriendCode;
                    BAUDataManager.SaveCheatData(Utils.GetHashPuid(player), player.Data.FriendCode, player.Data.PlayerName, "cheatData", "Sicko Menu RPC");
                    BAUDataManager.SaveCheatData(Utils.GetHashPuid(player), player.Data.FriendCode, player.Data.PlayerName, "sickoData", "Sicko Menu RPC");
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
                        BAUNotificationManager.NotifyCheat(player, $"AUM", newText: "Has been detected with a cheat client");
                        PlayerData[Utils.GetHashPuid(player)] = player.FriendCode;
                        AUMData[Utils.GetHashPuid(player)] = player.FriendCode;
                        BAUDataManager.SaveCheatData(Utils.GetHashPuid(player), player.Data.FriendCode, player.Data.PlayerName, "cheatData", "AUM RPC");
                        BAUDataManager.SaveCheatData(Utils.GetHashPuid(player), player.Data.FriendCode, player.Data.PlayerName, "aumData", "AUM RPC");
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
                    BAUNotificationManager.NotifyCheat(AUMPlayer, $"AUM", newText: "Has been detected with a cheat client");
                    PlayerData[Utils.GetHashPuid(AUMPlayer)] = AUMPlayer.FriendCode;
                    AUMData[Utils.GetHashPuid(AUMPlayer)] = AUMPlayer.FriendCode;
                    BAUDataManager.SaveCheatData(Utils.GetHashPuid(player), player.Data.FriendCode, player.Data.PlayerName, "cheatData", "AUM Chat RPC");
                    BAUDataManager.SaveCheatData(Utils.GetHashPuid(player), player.Data.FriendCode, player.Data.PlayerName, "aumData", "AUM Chat RPC");
                }
            }
            catch { }

            return;
        }
    }

    // Check and notify for invalid rpcs
    public static void CheckRPC(PlayerControl player, byte callId, MessageReader reader)
    {
        try
        {
            if (PlayerControl.LocalPlayer == null || player == null || player == PlayerControl.LocalPlayer || reader == null || !IsEnabled || !Main.AntiCheat.Value) return;

            RoleTypes? Role = player?.Data?.RoleType;
            Role ??= RoleTypes.Crewmate;
            bool IsImpostor = player.IsImpostorTeam();
            bool IsCrewmate = !player.IsImpostorTeam();
            string hashPuid = Utils.GetHashPuid(player);

            if (callId is (byte)RpcCalls.EnterVent or (byte)RpcCalls.ExitVent)
            {
                if ((IsCrewmate && Role != RoleTypes.Engineer) || player.Data.IsDead)
                    BAUNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");

                return;
            }

            if (callId is (byte)RpcCalls.CheckProtect or (byte)RpcCalls.ProtectPlayer)
            {
                if (Role is not RoleTypes.GuardianAngel)
                    BAUNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");

                return;
            }

            if (callId is (byte)RpcCalls.CheckMurder or (byte)RpcCalls.MurderPlayer)
            {
                PlayerControl target = reader.ReadNetObject<PlayerControl>();

                if (IsCrewmate || !player.IsAlive() || !target.IsAlive() || target.IsImpostorTeam())
                    BAUNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");

                return;
            }

            if (callId is (byte)RpcCalls.SendChat or (byte)RpcCalls.SendQuickChat)
            {
                if (player.IsAlive() && GameStates.InGame && !GameStates.IsLobby && !GameStates.IsMeeting && !GameStates.IsExilling && GameStates.IsInTask)
                    BAUNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");

                return;
            }

            if (callId is (byte)RpcCalls.SetLevel)
            {
                if (reader.BytesRemaining > 0)
                {
                    uint level = reader.ReadPackedUInt32();
                    if (level >= 200)
                    {
                        BAUNotificationManager.NotifyCheat(player, $"Invalid Level: {level}");
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
                        BAUNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)} - {Enum.GetName((SystemTypes)callId)}");
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
                            BAUNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)} - {Enum.GetName((SystemTypes)callId)}");
                        }
                    }
                }

                return;
            }

            if (IsImpostor)
            {
                if (callId is (byte)RpcCalls.CompleteTask or (byte)RpcCalls.BootFromVent)
                {
                    BAUNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                }

                return;
            }

            if (callId is (byte)RpcCalls.Pet or (byte)RpcCalls.CancelPet)
            {
                if (player?.CurrentOutfit?.PetId == "pet_EmptyPet")
                {
                    BAUNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
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
    public static bool CheckCancelRPC(PlayerControl player, byte callId, MessageReader reader)
    {
        try
        {
            if (PlayerControl.LocalPlayer == null || player == null || player == PlayerControl.LocalPlayer || reader == null || !IsEnabled || !Main.AntiCheat.Value) return true;

            RoleTypes Role = player.Data.RoleType;
            bool IsImpostor = player.IsImpostorTeam();
            bool IsCrewmate = !player.IsImpostorTeam();

            if (TrustedRPCs(callId) != true)
            {
                BAUNotificationManager.NotifyCheat(player, $"Untrusted RPC received: {callId}");
                return false;
            }

            if (callId is (byte)RpcCalls.CheckMurder or (byte)RpcCalls.MurderPlayer)
            {
                PlayerControl target = reader.ReadNetObject<PlayerControl>();

                if (!target.IsAlive() || target.IsImpostorTeam())
                {
                    BAUNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                    return false;
                }
            }

            if (callId is (byte)RpcCalls.CheckShapeshift or (byte)RpcCalls.Shapeshift)
            {
                var flag = reader.ReadBoolean();

                if (Role is not RoleTypes.Shapeshifter || !player.IsAlive())
                {
                    BAUNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                    return false;
                }

                if (!flag && !player.IsInVent())
                {
                    BAUNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                    return false;
                }
            }

            if (callId is (byte)RpcCalls.CheckVanish or (byte)RpcCalls.CheckAppear or (byte)RpcCalls.StartVanish or (byte)RpcCalls.StartAppear)
            {
                if (Role is not RoleTypes.Phantom || player.Data.IsDead)
                {
                    BAUNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                    return false;
                }
            }

            if (!player.IsHost())
            {
                if (callId is (byte)RpcCalls.SetTasks
                or (byte)RpcCalls.ExtendLobbyTimer
                or (byte)RpcCalls.CloseMeeting)
                {
                    BAUNotificationManager.NotifyCheat(player, $"Invalid Host RPC: {Enum.GetName((RpcCalls)callId)}");
                    return false;
                }
            }

            if (player.DataIsCollected() == true)
            {
                if (callId is (byte)RpcCalls.SetName or (byte)RpcCalls.CheckName or (byte)RpcCalls.SetLevel)
                {
                    BAUNotificationManager.NotifyCheat(player, $"Invalid Set RPC: {Enum.GetName((RpcCalls)callId)}");
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
                    BAUNotificationManager.NotifyCheat(player, $"Invalid Set RPC: {Enum.GetName((RpcCalls)callId)}");
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
                    BAUNotificationManager.NotifyCheat(player, $"Invalid Lobby RPC: {Enum.GetName((RpcCalls)callId)}");
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
