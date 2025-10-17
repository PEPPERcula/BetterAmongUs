using AmongUs.GameOptions;
using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Managers;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class MurderPlayerHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.MurderPlayer;

    // Prevent ban exploit
    internal override bool HandleAntiCheatCancel(PlayerControl? player, MessageReader reader)
    {
        PlayerControl target = reader.ReadNetObject<PlayerControl>();

        if (target != null)
        {
            if (target.IsLocalPlayer() && GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown) > 2.5f)
            {
                target.BetterData().AntiCheatInfo.TimesAttemptedKilled++;

                if (target.BetterData().AntiCheatInfo.TimesAttemptedKilled >= 5 && !target.IsAlive())
                {
                    if (BetterNotificationManager.NotifyCheat(player, string.Format(Translator.GetString("AntiCheat.InvalidAction"), Translator.GetString("AntiCheat.TryBanExploit"))))
                    {
                        LogRpcInfo($"{target.BetterData().AntiCheatInfo.TimesAttemptedKilled >= 5} && {!target.IsAlive()}");
                    }
                    return false;
                }

                // Cancel murder on client if not alive
                if (!target.IsAlive())
                {
                    return false;
                }
            }
        }

        return true;
    }

    internal override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        PlayerControl target = reader.ReadNetObject<PlayerControl>();

        if (target != null)
        {
            if (!sender.IsImpostorTeam() || !sender.IsAlive() || sender.IsInVanish() || target.IsImpostorTeam())
            {
                if (BetterNotificationManager.NotifyCheat(sender, GetFormatActionText()))
                {
                    LogRpcInfo($"{!sender.IsImpostorTeam()} || {sender.IsInVanish()}" +
                        $" || {!target.IsAlive()} || {target.IsImpostorTeam()}");
                }
            }
        }
    }
}
