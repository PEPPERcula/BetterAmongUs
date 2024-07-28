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
    public static void HandleRPCBeforeCheck(PlayerControl player, byte callId, MessageReader Oldreader)
    {
        MessageReader reader = Oldreader;

        if (player == PlayerControl.LocalPlayer || PlayerControl.LocalPlayer == null || player == null || !IsEnabled) return;

        if (callId is unchecked((byte)CustomRPC.Sicko) && Main.AntiCheat.Value)
        {
            if (reader.BytesRemaining == 0)
            {
                var flag = SickoData.ContainsKey(Utils.GetHashPuid(player));

                if (reader.BytesRemaining == 0 && !flag)
                {
                    AmongUsClient.Instance.ReportPlayer(player.GetClientId(), ReportReasons.Cheating_Hacking);
                    BetterNotificationManager.NotifyCheat(player, $"Sicko Menu", newText: "Has been detected with a cheat client");
                    PlayerData[Utils.GetHashPuid(player)] = player.FriendCode;
                    SickoData[Utils.GetHashPuid(player)] = player.FriendCode;
                    BetterDataManager.SaveCheatData(Utils.GetHashPuid(player), player.Data.FriendCode, player.Data.PlayerName, "cheatData", "Sicko Menu RPC");
                    BetterDataManager.SaveCheatData(Utils.GetHashPuid(player), player.Data.FriendCode, player.Data.PlayerName, "sickoData", "Sicko Menu RPC");
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
                        BetterNotificationManager.NotifyCheat(player, $"AUM", newText: "Has been detected with a cheat client");
                        PlayerData[Utils.GetHashPuid(player)] = player.FriendCode;
                        AUMData[Utils.GetHashPuid(player)] = player.FriendCode;
                        BetterDataManager.SaveCheatData(Utils.GetHashPuid(player), player.Data.FriendCode, player.Data.PlayerName, "cheatData", "AUM RPC");
                        BetterDataManager.SaveCheatData(Utils.GetHashPuid(player), player.Data.FriendCode, player.Data.PlayerName, "aumData", "AUM RPC");
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
                    BetterNotificationManager.NotifyCheat(AUMPlayer, $"AUM", newText: "Has been detected with a cheat client");
                    PlayerData[Utils.GetHashPuid(AUMPlayer)] = AUMPlayer.FriendCode;
                    AUMData[Utils.GetHashPuid(AUMPlayer)] = AUMPlayer.FriendCode;
                    BetterDataManager.SaveCheatData(Utils.GetHashPuid(player), player.Data.FriendCode, player.Data.PlayerName, "cheatData", "AUM Chat RPC");
                    BetterDataManager.SaveCheatData(Utils.GetHashPuid(player), player.Data.FriendCode, player.Data.PlayerName, "aumData", "AUM Chat RPC");
                }
            }
            catch { }

            return;
        }
    }

    // Check and notify for invalid rpcs
    public static void CheckRPC(PlayerControl player, byte callId, MessageReader Oldreader)
    {
        MessageReader reader = Oldreader;
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
                        if (IsCrewmate || !player.IsAlive() || player.IsInVanish() || !target.IsAlive() || target.IsImpostorTeam())
                            BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                    }
                }

                return;
            }

            if (callId is (byte)RpcCalls.SendChat or (byte)RpcCalls.SendQuickChat)
            {
                if (player.IsAlive() && GameStates.IsInGamePlay && !GameStates.IsMeeting && !GameStates.IsExilling)
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");

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
        MessageReader reader = Oldreader;
        try
        {
            if (PlayerControl.LocalPlayer == null || player == null || player == PlayerControl.LocalPlayer || reader == null || !IsEnabled || !Main.AntiCheat.Value) return true;

            RoleTypes Role = player.Data.RoleType;
            bool IsImpostor = player.IsImpostorTeam();
            bool IsCrewmate = !player.IsImpostorTeam();

            if (TrustedRPCs(callId) != true)
            {
                BetterNotificationManager.NotifyCheat(player, $"Untrusted RPC received: {callId}");
                return false;
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
