using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
using BetterAmongUs.Patches;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

public class SetLevelHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SetLevel;

    public override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        if (sender.DataIsCollected() == true && sender.BetterData().AntiCheatInfo.HasSetLevel && !GameState.IsLocalGame && GameState.IsVanillaServer)
        {
            BetterNotificationManager.NotifyCheat(sender, GetFormatSetText());
            LogRpcInfo($"{sender.DataIsCollected() == true} && {!GameState.IsLocalGame} && {GameState.IsVanillaServer}");

            return false;
        }

        sender.BetterData().AntiCheatInfo.HasSetLevel = true;

        return true;
    }

    public override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        uint level = reader.ReadPackedUInt32();

        if (level + 1 > BetterGameSettings.DetectedLevelAbove.GetInt())
        {
            BetterNotificationManager.NotifyCheat(sender, string.Format(Translator.GetString("AntiCheat.InvalidLevelRPC"), level));
            LogRpcInfo($"{level > BetterGameSettings.DetectedLevelAbove.GetInt()} - {level} > {BetterGameSettings.DetectedLevelAbove.GetInt()}");
        }
    }
}
