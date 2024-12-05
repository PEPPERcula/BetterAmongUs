using BetterAmongUs.Helpers;
using BetterAmongUs.Patches;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

public class UpdateSystemHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.UpdateSystem;

    public SystemTypes CatchedSystemType;
    public override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        byte amount = reader.ReadByte();
        byte hostNum = 128; // Only host should ever send this number
        byte singleFixNum = 0;
        byte minLightNum = 0;
        byte MaxLightNum = 4;
        byte fixNum = 16; // 16 and 17
        byte openPanelNum = 64; // 64 and 65
        byte closePanelNum = 32; // 32 and 33

        // Set fixing status
        if (Utils.SystemTypeIsSabotage(CatchedSystemType))
        {
            if (amount == openPanelNum && !sender.BetterData().AntiCheatInfo.IsFixingPanelSabotage)
            {
                sender.BetterData().AntiCheatInfo.OpenSabotageNum = 1;
            }
            else if (amount == openPanelNum + 1 && !sender.BetterData().AntiCheatInfo.IsFixingPanelSabotage)
            {
                sender.BetterData().AntiCheatInfo.OpenSabotageNum = 2;
            }

            if (amount == closePanelNum && sender.BetterData().AntiCheatInfo.OpenSabotageNum == 1)
            {
                sender.BetterData().AntiCheatInfo.OpenSabotageNum = 0;
            }
            else if (amount == closePanelNum + 1 && sender.BetterData().AntiCheatInfo.OpenSabotageNum == 2)
            {
                sender.BetterData().AntiCheatInfo.OpenSabotageNum = 0;
            }
        }

        if (!GameState.IsHost || PlayerControl.LocalPlayer == null || sender == null || sender.IsLocalPlayer() || sender.BetterData().IsBetterHost || !BAUAntiCheat.IsEnabled || !Main.AntiCheat.Value
            || GameState.IsBetterHostLobby && !GameState.IsHost || !BetterGameSettings.DetectInvalidRPCs.GetBool()) return true;

        if (!BetterGameSettings.CancelInvalidSabotage.GetBool())
            return true;

        // Single Fix: 0
        // Lights: 0-1-2-3-4
        // Fix 1: 16
        // Fix 2: 17
        // Panel 1: Open/Hold 64 - Close/Release 32
        // Panel 2: Open/Hold 65 - Close/Release 33
        // Host: 128

        // Activate sabotage
        if (CatchedSystemType == SystemTypes.Sabotage)
        {
            SystemTypes SaboType = (SystemTypes)amount;

            if (!sender.IsImpostorTeam() || GameState.IsAnySabotageActive() || !Utils.SystemTypeIsSabotage(SaboType)
                || GameState.SkeldIsActive && ShipStatus.Instance.AllDoors.Any(door => !door.IsOpen))
            {
                /*
                BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName(SaboType)}");
                Logger.LogCheat($"{player.BetterData().RealName} {Enum.GetName(SaboType)}: {!player.IsImpostorTeam()} || {GameStates.IsAnySabotageActive()} " +
                    $"|| {!Utils.SystemTypeIsSabotage(SaboType)}");
                */
                return false;
            }
        }

        // Fix sabotage
        else if (Utils.SystemTypeIsSabotage(CatchedSystemType))
        {
            if (amount == hostNum && !sender.IsHost())
            {
                /*
                BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName(systemType)}");
                Logger.LogCheat($"{player.BetterData().RealName} {Enum.GetName(systemType)}: {amount == hostNum} && {!player.IsHost()}");
                */
                return false;
            }

            if (!GameState.IsSystemActive(CatchedSystemType))
            {
                if (CatchedSystemType is SystemTypes.Electrical)
                {
                    /*
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName(systemType)}");
                    Logger.LogCheat($"{player.BetterData().RealName} {Enum.GetName(systemType)}: {systemType is SystemTypes.Electrical}");
                    */
                }

                return false;
            }

            if (sender.BetterData().AntiCheatInfo.IsFixingPanelSabotage)
            {
                if (sender.BetterData().AntiCheatInfo.OpenSabotageNum == 0)
                {
                    /*
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName(systemType)}");
                    Logger.LogCheat($"{player.BetterData().RealName} {Enum.GetName(systemType)}: {player.BetterData().AntiCheatInfo.OpenSabotageNum == 0}");
                    */
                    return false;
                }

                if (amount == openPanelNum && amount == openPanelNum + 1)
                {
                    /*
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName(systemType)}");
                    Logger.LogCheat($"{player.BetterData().RealName} {Enum.GetName(systemType)}: {amount == openPanelNum} && {amount == openPanelNum + 1}");
                    */
                    return false;
                }

                if (sender.BetterData().AntiCheatInfo.OpenSabotageNum == 1 && amount == openPanelNum + 1
                    || sender.BetterData().AntiCheatInfo.OpenSabotageNum == 2 && amount == openPanelNum)
                {
                    /*
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName(systemType)}");
                    Logger.LogCheat($"{player.BetterData().RealName} {Enum.GetName(systemType)}: {player.BetterData().AntiCheatInfo.OpenSabotageNum == 1} && {amount == openPanelNum + 1} " +
                        $"|| {player.BetterData().AntiCheatInfo.OpenSabotageNum == 2} && {amount == openPanelNum}");
                    */
                    return false;
                }
            }
            else
            {
                if (amount != fixNum && amount != fixNum + 1
                    && amount != closePanelNum && amount != closePanelNum + 1
                    && amount > MaxLightNum)
                {
                    /*
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName(systemType)}");
                    Logger.LogCheat($"{player.BetterData().RealName} {Enum.GetName(systemType)}: {amount != fixNum} && {amount != fixNum + 1}" +
                        $" && {amount != closePanelNum} && {amount != closePanelNum + 1}" +
                        $" && {amount > MaxLightNum}");
                    */
                    return false;
                }

                if (CatchedSystemType == SystemTypes.Electrical)
                {
                    if (amount > MaxLightNum)
                    {
                        /*
                        BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName(systemType)}");
                        Logger.LogCheat($"{player.BetterData().RealName} {Enum.GetName(systemType)}: {amount > MaxLightNum}");
                        */
                        return false;
                    }
                }
            }
        }

        return true;
    }
}
