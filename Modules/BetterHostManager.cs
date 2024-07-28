using Hazel;
using InnerNet;
using System.Text;
using System.Text.RegularExpressions;

namespace BetterAmongUs;

class BetterHostManager
{
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
                        if (player.Data.Role is GuardianAngelRole && player.IsAlive() && !player.IsImpostorTeam())
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
                        if (player.IsAlive() && player.IsImpostorTeam() && !player.inMovingPlat && !player.IsInVent() && !player.IsInVanish() && !player.shapeshifting && !player.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
                        {
                            if (target.IsAlive() && !target.IsImpostorTeam() && !target.inMovingPlat && !target.IsInVent() && !target.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
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
                        if (player.Data.Role is ShapeshifterRole && player.IsAlive() && player.IsImpostorTeam() && !player.inMovingPlat && !player.shapeshifting && !player.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
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
                    if (player.Data.Role is PhantomRole && player.IsAlive() && player.IsImpostorTeam() && !player.IsInVent() && !player.inMovingPlat && !player.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
                    {
                        player.RpcVanish();
                        break;
                    }

                    canceled = true;
                }
                break;

            case (byte)RpcCalls.CheckAppear:
                {
                    bool flag = reader.ReadBoolean();

                    if (player.Data.Role is PhantomRole && player.IsAlive() && player.IsImpostorTeam() && !player.inMovingPlat && !player.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
                    {
                        if (!player.IsInVent() && flag == false)
                        {
                            break;
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
        if (player == null || !player.DataIsCollected()) return;

        if (!Main.BetterHost.Value)
        {
            if (player.Data.PlayerName != player.CurrentOutfit.PlayerName && !player.IsInShapeshift() || force)
            {
                player.RpcSetName(player.Data.PlayerName);
                Logger.Log($"Reset {player.Data.PlayerName} name", "RPC");
            }
            return;
        }

        if (!IsBetterHost && !force) return;

        string friendCode = player.Data.FriendCode;

        string pattern = @"^[a-zA-Z0-9#]+$";
        string hashtagPattern = @"^#[0-9]{4}$";
        string friendCodeColor = (Regex.Replace(friendCode, hashtagPattern, string.Empty).Length is > 10 or < 5 || !Regex.IsMatch(friendCode, pattern) || !Regex.IsMatch(friendCode, hashtagPattern)) ? "#00f7ff" : "#ff0000";

        foreach (PlayerControl target in Main.AllPlayerControls)
        {
            if (target == null || target == PlayerControl.LocalPlayer || target.GetIsBetterUser()) continue;

            string NewName = player.CurrentOutfit.PlayerName;
            string BottomTag = "";

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

                BottomTag += $"<color={friendCodeColor}>{friendCode}</color>";
            }
            else if (GameStates.IsInGame && !GameStates.IsLobby)
            {
                string Role = $"<color={player.GetTeamHexColor()}>{player.GetRoleName()}</color>";
                if (!player.IsImpostorTeam() && !target.IsImpostorTeam())
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

            NewName = $"<size=65%>{sbTopInfo}</size>\n{NewName}\n<size=65%>{BottomTag}</size>";

            player.RpcSetNamePrivate(NewName, target);
            Logger.Log($"Set {player.Data.PlayerName} name to {NewName.Replace("\n", "-")} for {target.Data.PlayerName}", "RPC");
        }
    }
}
