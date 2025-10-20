using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils;

using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using BetterAmongUs.Network;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using System.Collections;
using System.Text;
using UnityEngine;

namespace BetterAmongUs.Managers;

internal static class HostManager
{
    internal enum SyncType
    {
        Reset,
        Gameplay,
        Meeting
    }

    private static Coroutine? syncCoroutine = null;

    internal static void SyncNames(SyncType syncType, float sDelay, int repeat = 1)
    {
        if (!GameState.IsHost || !BetterGameSettings.ShowRoleForClients.GetValue()) return;

        if (syncCoroutine != null && GameData.Instance != null)
        {
            GameData.Instance.StopCoroutine(syncCoroutine);
            syncCoroutine = null;
        }

        if (GameData.Instance != null)
        {
            syncCoroutine = GameData.Instance.StartCoroutine(CoSyncNamesDelay(syncType, sDelay, repeat));
        }
    }

    private static IEnumerator CoSyncNamesDelay(SyncType syncType, float sDelay, int repeat)
    {
        for (int i = 0; i < repeat; i++)
        {
            yield return new WaitForSeconds(sDelay);
            SyncNames(syncType);
        }

        syncCoroutine = null;
    }

    internal static void SyncNames(SyncType syncType)
    {
        if (!GameState.IsHost || !BetterGameSettings.ShowRoleForClients.GetValue()) return;

        StringBuilder sb = new();

        foreach (var player in BAUPlugin.AllPlayerControls)
        {
            if (syncType is SyncType.Reset)
            {
                SyncResetName(player);
                continue;
            }

            foreach (var target in BAUPlugin.AllPlayerControls)
            {
                if (target.IsLocalPlayer() || target.BetterData().IsVerifiedBetterUser) continue;

                switch (syncType)
                {
                    case SyncType.Gameplay:
                    case SyncType.Meeting:
                        var shapeshift = player.shapeshiftTargetPlayerId == -1 ? player.Data : Utils.PlayerDataFromPlayerId(player.shapeshiftTargetPlayerId);
                        SyncNamesForGameplay(shapeshift, target, sb, player.Data, syncType == SyncType.Meeting);
                        break;
                }

                var newName = sb.ToString();
                sb.Clear();

                if (player.BetterPlayerControl().NameSetLastFor.TryGetValue(target.Data, out var lastName) && lastName == newName)
                    continue;

                player.BetterPlayerControl().NameSetLastFor[target.Data] = newName;

                RPC.RpcSetNameForTarget(player, newName, target);
                Logger.Warning($"Setting {player.BetterData().RealName} name to {newName}, for {target.BetterData().RealName}");
            }
        }
    }

    private static void SyncResetName(PlayerControl player)
    {
        player.RpcSetName(player.BetterData().RealName);
    }

    private static void SyncNamesForGameplay(NetworkedPlayerInfo playerData, PlayerControl target, StringBuilder sb, NetworkedPlayerInfo playerNonShapeshift, bool isMeeting)
    {
        bool hasInfo = false;

        if ((playerNonShapeshift.IsImpostorTeam() && target.IsImpostorTeam())
            || playerNonShapeshift.PlayerId == target.Data.PlayerId // Self
            || (!target.IsAlive() && !target.Is(RoleTypes.GuardianAngel)))
        {
            string roleInfo = $"<color={playerNonShapeshift.GetTeamHexColor()}>{playerNonShapeshift.RoleType.GetRoleName()}</color>";

            if (!playerNonShapeshift.IsImpostorTeam() && playerNonShapeshift.Tasks.Count > 0)
            {
                int completedTasks = playerNonShapeshift.Tasks.ToArray().Count(task => task.Complete);
                roleInfo += $" <color=#cbcbcb>({completedTasks}/{playerNonShapeshift.Tasks.Count})</color>";
            }

            sb.Append(roleInfo.Size(60f));
            hasInfo = true;
        }

        if (hasInfo)
            sb.Append('\n');

        sb.Append($"{playerData.BetterData().RealName}");

        if (hasInfo && !isMeeting)
            sb.Append("\n ");
    }

    internal static void SetCustomEjectMessage(string msg)
    {
    }
}
