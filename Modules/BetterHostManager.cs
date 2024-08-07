using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using UnityEngine;
using System.Text;

namespace BetterAmongUs;

class BetterHostManager
{
    private static readonly List<PlayerControl> VentStuck = [];

    public static void PlayerUpdate(PlayerControl player)
    {
        // Lock up player vent button if it's invalid vent
        if (!VentStuck.Contains(player) && player.inVent && !player.Is(RoleTypes.Engineer) && !player.IsImpostorTeam())
        {
            player.MyPhysics.RpcEnterVent(player.GetPlayerVentId());
            player.MyPhysics.RpcBootFromVent(player.GetPlayerVentId());
            VentStuck.Add(player);
        }
    }

    public static bool CheckRange(Vector2 pos1, Vector2 pos2, float range) => Vector2.Distance(pos1, pos2) <= range;

    public static bool CheckRPCAsHost(PlayerControl player, byte callId, MessageReader reader, ref bool canceled)
    {
        if (player == null || !GameStates.IsHost) return true;

        bool shouldReturn = false;

        switch (callId)
        {
            case (byte)RpcCalls.CheckProtect:
                {
                    PlayerControl target = reader.ReadNetObject<PlayerControl>();
                    if (target != null)
                    {
                        if (player.Is(RoleTypes.GuardianAngel)
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

                    bool condition = false;

                    if (player.BetterData().TimeSinceKill >= GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown)
                    {
                        condition = true;
                    }

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
                            && condition
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
                        if (player.Is(RoleTypes.Shapeshifter)
                            && player.IsAlive()
                            && player.IsImpostorTeam()
                            && !player.inMovingPlat
                            && !player.shapeshifting
                            && !player.onLadder
                            && !player.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
                        {
                            if (!target.IsInVent() && !GameStates.IsMeeting && !GameStates.IsExilling && flag == false)
                            {
                                break;
                            }

                            player.RpcShapeshift(target, !target.IsInVent() && !GameStates.IsMeeting && !GameStates.IsExilling);
                            break;
                        }
                    }

                    canceled = true;
                }
                break;

            case (byte)RpcCalls.CheckVanish:
                {
                    if (player.Is(RoleTypes.Phantom)
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

                    if (player.Is(RoleTypes.Phantom)
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
            if (target == null || target == PlayerControl.LocalPlayer || target.BetterData().IsBetterUser) continue;

            string NewName = player.CurrentOutfit.PlayerName;

            StringBuilder sbTopTag = new StringBuilder();
            StringBuilder sbTopInfo = new StringBuilder();

            if (GameStates.IsLobby)
            {
                NewName = player.Data.PlayerName;

                if (player.IsHost())
                    NewName = player.GetPlayerNameAndColor();

                if (player.IsDev())
                    sbTopTag.Append("<color=#6e6e6e>(<color=#0088ff>Dev</color>)</color>+++");

                if (player == PlayerControl.LocalPlayer)
                    sbTopTag.Append($"<color=#0dff00>Better Host</color>+++");
                else if (player.BetterData().IsBetterUser)
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
                NewName = $"<size=65%>{sbTopInfo}</size>\n{NewName}";
            else
                NewName = $"{NewName}";

            // Don't send rpc if name is the same!
            if (player.BetterData().LastNameSetFor.TryGetValue(target.PlayerId, out var lastName) && lastName == NewName)
            {
                if (!force)
                {
                    return;
                }
            }

            player.BetterData().LastNameSetFor[target.PlayerId] = NewName;

            player.RpcSetNamePrivate(NewName, target);
            Logger.Log($"Set {player.Data.PlayerName} name to {NewName.Replace("\n", "-")} for {target.Data.PlayerName}", "RPC");
        }
    }
}
