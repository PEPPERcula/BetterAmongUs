using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using UnityEngine;
using System.Text;

namespace BetterAmongUs;

class BetterHostManager
{
    private static Dictionary<byte, Dictionary<byte, string>> LastPlayerName = []; // Targetid, Playerid, Name
    private static Dictionary<PlayerControl, Vector2> LastPlayerPos = [];
    private static Dictionary<PlayerControl, Vector2> LastPlayerPosDelay = [];
    private static Dictionary<PlayerControl, float> LastPlayeridealSpeed = [];
    private static Dictionary<PlayerControl, ushort> LastPlayerMoveSequence = [];
    private static List<PlayerControl> VentStuck = [];

    public static void Update(PlayerControl player)
    {
        if (!Main.BetterHost.Value) return;

        var idealSpeed = player.NetTransform.idealSpeed;

        // Prevent player from going over speed modifier
        if (player.IsAlive() && player.CanMove && player.MyPhysics.Animations.IsPlayingRunAnimation() && IsSpeedExceeding(idealSpeed) == true)
        {
            if (LastPlayerPosDelay.ContainsKey(player) && LastPlayeridealSpeed.ContainsKey(player))
            {
                if ((int)idealSpeed >= (int)LastPlayeridealSpeed[player])
                {
                    player.RpcTeleport(LastPlayerPosDelay[player]);
                    player.NetTransform.idealSpeed = 0f;
                    Logger.Log($"invalid move speed, {idealSpeed} is > {GetAverageSpeed()}, reset {player.Data.PlayerName} pos:{player.GetCustomPosition()}");
                }
            }
        }

        LastPlayeridealSpeed[player] = idealSpeed;

        var lastPos = player.GetCustomPosition();

        // Get delayed position
        _ = new LateTask(() =>
        {
            if (player != null)
            {
                LastPlayerPosDelay[player] = lastPos;
            }
        }, 1.8f, shoudLog: false);

        // Prevent player from moving in certain States
        if (player.CanMove)
        {
            LastPlayerPos[player] = player.GetCustomPosition();
            LastPlayerMoveSequence[player] = player.NetTransform.lastSequenceId;
        }
        else
        {
            if (player.shapeshifting)
            {
                if (LastPlayerPos.ContainsKey(player))
                {
                    if (LastPlayerMoveSequence[player] + 6 < player.NetTransform.lastSequenceId)
                    {
                        player.RpcTeleport(LastPlayerPos[player]);
                        Logger.Log($"invalid move sequence, reset {player.Data.PlayerName} pos:{player.GetCustomPosition()}");
                        LastPlayerMoveSequence[player] = (ushort)(player.NetTransform.lastSequenceId - 4);
                    }
                }
            }
        }

        // Lock up player vent button if it's invalid vent
        if (!VentStuck.Contains(player) && player.inVent && player.Data.RoleType != RoleTypes.Engineer && !player.IsImpostorTeam())
        {
            player.MyPhysics.RpcEnterVent(player.GetPlayerVentId());
            player.MyPhysics.RpcBootFromVent(player.GetPlayerVentId());
            VentStuck.Add(player);
        }
    }

    public static bool IsSpeedExceeding(float currentSpeed)
    {
        float leniency = 1.8f;
        float thresholdSpeed = GetAverageSpeed() + leniency;
        bool isExceeding = currentSpeed > thresholdSpeed;
        return isExceeding;
    }

    public static float GetAverageSpeed()
    {
        float speedMod = GameStates.IsHideNSeek
            ? GameOptionsManager.Instance.currentHideNSeekGameOptions.PlayerSpeedMod
            : GameOptionsManager.Instance.currentNormalGameOptions.PlayerSpeedMod;

        // Data points
        float[] mods = { 0.5f, 1.5f, 2.0f }; // Add more as needed
        float[] avgSpeeds = { 1.3f, 3.75f, 5.0f }; // Add corresponding speeds

        int n = mods.Length;
        float sumMod = 0, sumAvgSpeed = 0, sumModAvgSpeed = 0, sumModSquared = 0;

        for (int i = 0; i < n; i++)
        {
            sumMod += mods[i];
            sumAvgSpeed += avgSpeeds[i];
            sumModAvgSpeed += mods[i] * avgSpeeds[i];
            sumModSquared += mods[i] * mods[i];
        }

        float m = (n * sumModAvgSpeed - sumMod * sumAvgSpeed) / (n * sumModSquared - sumMod * sumMod);
        float c = (sumAvgSpeed - m * sumMod) / n;

        float estimatedAverageSpeed = m * speedMod + c;

        return estimatedAverageSpeed;
    }

    public static bool CheckRange(Vector2 pos1, Vector2 pos2, float range) => Vector2.Distance(pos1, pos2) <= range;

    public static bool CheckRPCAsHost(PlayerControl player, byte callId, MessageReader reader, ref bool canceled)
    {
        if (player == null) return true;

        bool shouldReturn = false;

        switch (callId)
        {
            case (byte)RpcCalls.CheckProtect:
                {
                    PlayerControl target = reader.ReadNetObject<PlayerControl>();
                    if (target != null)
                    {
                        if (player.Data.RoleType == RoleTypes.GuardianAngel
                            && !player.IsAlive()
                            && !player.IsImpostorTeam()
                            && CheckRange(player.GetCustomPosition(), target.GetCustomPosition(), 3f))
                        {
                            if (target.IsAlive())
                            {
                                player.RpcProtectPlayer(target, player.Data.DefaultOutfit.ColorId);
                                break;
                            }
                        }
                    }

                    canceled = true;
                }
                break;

            case (byte)RpcCalls.CheckMurder:
                {
                    PlayerControl target = reader.ReadNetObject<PlayerControl>();

                    if (target != null)
                    {
                        if (player.IsAlive()
                            && player.IsImpostorTeam()
                            && !player.inMovingPlat
                            && !player.IsInVent()
                            && !player.IsInVanish()
                            && !player.shapeshifting
                            && !player.onLadder
                            && !player.MyPhysics.Animations.IsPlayingAnyLadderAnimation()
                            && CheckRange(player.GetCustomPosition(), target.GetCustomPosition(), 3f))
                        {
                            if (target.IsAlive()
                                && !target.IsImpostorTeam()
                                && !target.inMovingPlat
                                && !target.IsInVent()
                                && !target.onLadder
                                && !target.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
                            {
                                player.RpcMurderPlayer(target, true);
                                break;
                            }
                        }
                    }

                    canceled = true;
                }
                break;

            case (byte)RpcCalls.CheckShapeshift:
                {
                    PlayerControl target = reader.ReadNetObject<PlayerControl>();
                    bool flag = reader.ReadBoolean();

                    if (target != null)
                    {
                        if (player.Data.RoleType == RoleTypes.Shapeshifter
                            && player.IsAlive()
                            && player.IsImpostorTeam()
                            && !player.inMovingPlat
                            && !player.shapeshifting
                            && !player.onLadder
                            && !player.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
                        {
                            if (!target.IsInVent() && flag == false)
                            {
                                break;
                            }

                            player.RpcShapeshift(target, !target.IsInVent());
                            break;
                        }
                    }

                    canceled = true;
                }
                break;

            case (byte)RpcCalls.CheckVanish:
                {
                    if (player.Data.RoleType == RoleTypes.Phantom
                        && player.IsAlive()
                        && player.IsImpostorTeam()
                        && !player.IsInVent()
                        && !player.inMovingPlat
                        && !player.onLadder
                        && !player.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
                    {

                        if (AmongUsClient.Instance.AmClient)
                        {
                            player.SetRoleInvisibility(true, true, true);
                        }
                        player.RpcVanish();

                        break;
                    }

                    canceled = true;
                }
                break;

            case (byte)RpcCalls.CheckAppear:
                {
                    bool flag = reader.ReadBoolean();

                    if (player.Data.RoleType == RoleTypes.Phantom
                        && player.IsAlive() && player.IsImpostorTeam()
                        && !player.inMovingPlat
                        && !player.onLadder
                        && !player.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
                    {
                        if (!player.IsInVent() && flag == false)
                        {
                            break;
                        }

                        if (AmongUsClient.Instance.AmClient)
                        {
                            player.SetRoleInvisibility(false, !player.IsInVent(), true);
                        }
                        player.RpcAppear(!player.IsInVent());

                        break;
                    }

                    canceled = true;
                }
                break;

            default:
                shouldReturn = true;
                break;
        }


        return shouldReturn;
    }

    // Set player names for better host
    public static void SetPlayersInfoAsHost(PlayerControl player, bool forMeeting = false, bool force = false, bool IsBetterHost = true)
    {
        if (player == null) return;

        if (!Main.BetterHost.Value || forMeeting)
        {
            if (player.Data.PlayerName != player.CurrentOutfit.PlayerName && !player.IsInShapeshift() || force)
            {
                player.RpcSetName(player.Data.PlayerName);
                Logger.Log($"Reset {player.Data.PlayerName} name", "RPC");
            }
            return;
        }

        if (!IsBetterHost && !force) return;

        foreach (PlayerControl target in Main.AllPlayerControls)
        {
            if (target == null || target == PlayerControl.LocalPlayer || target.GetIsBetterUser()) continue;

            string NewName = player.CurrentOutfit.PlayerName;

            StringBuilder sbTopTag = new StringBuilder();
            StringBuilder sbTopInfo = new StringBuilder();

            if (GameStates.IsLobby)
            {
                NewName = player.Data.PlayerName;

                if (player.IsDev())
                    sbTopTag.Append("<color=#6e6e6e>(<color=#0088ff>Dev</color>)</color>+++");

                if (player == PlayerControl.LocalPlayer)
                    sbTopTag.Append($"<color=#0dff00>Better Host</color>+++");
                else if (player.GetIsBetterUser())
                    sbTopTag.Append("<color=#0dff00>Better User</color>+++");
            }
            else if (GameStates.IsInGamePlay)
            {
                string Role = $"<color={player.GetTeamHexColor()}>{player.GetRoleName()}</color>";
                if (player.IsImpostorTeam() is false || target.IsImpostorTeam() is false)
                {
                    if (target.IsAlive() && player != target)
                    {
                        Role = "";
                    }
                }

                sbTopTag.Append($"{Role}+++");
            }

            for (int i = 0; i < sbTopTag.ToString().Split("+++").Length; i++)
            {
                if (!string.IsNullOrEmpty(sbTopTag.ToString().Split("+++")[i]))
                {
                    if (i < sbTopTag.ToString().Split("+++").Length)
                    {
                        sbTopInfo.Append(sbTopTag.ToString().Split("+++")[i]);
                    }
                    if (i != sbTopTag.ToString().Split("+++").Length - 2)
                    {
                        sbTopInfo.Append(" - ");
                    }
                }
            }

            if (sbTopInfo.Length > 0)
                NewName = $"<size=50%>{sbTopInfo}</size>\n{NewName}";
            else
                NewName = $"{NewName}";

            // Don't send rpc if name is the same!
            if (LastPlayerName.TryGetValue(target.Data.PlayerId, out var playerNameDict))
            {
                if (playerNameDict.TryGetValue(player.Data.PlayerId, out var currentName) && currentName == NewName)
                {
                    if (!force)
                    {
                        return;
                    }
                }
            }
            else
            {
                LastPlayerName[target.Data.PlayerId] = new Dictionary<byte, string>();
            }

            LastPlayerName[target.Data.PlayerId][player.Data.PlayerId] = NewName;

            player.RpcSetNamePrivate(NewName, target);
            Logger.Log($"Set {player.Data.PlayerName} name to {NewName.Replace("\n", "-")} for {target.Data.PlayerName}", "RPC");
        }
    }
}
