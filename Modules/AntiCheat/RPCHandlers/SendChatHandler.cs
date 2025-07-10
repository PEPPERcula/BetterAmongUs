using AmongUs.Data;
using BetterAmongUs.Data;
using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Managers;
using BetterAmongUs.Patches;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class SendChatHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SendChat;

    internal override void Handle(PlayerControl? sender, MessageReader reader)
    {
        var text = reader.ReadString();

        // Check banned words
        if (BetterGameSettings.UseBanWordList.GetBool() && (!BetterGameSettings.UseBanWordListOnlyLobby.GetBool() || GameState.IsLobby))
        {
            if (TextFileHandler.CompareStringFilters(BetterDataManager.banWordListFile, text.Split(' ')))
            {
                sender.Kick(false, $"has been kicked due to\nchat message containing a banned word!");
            }
        }
    }

    internal override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        if (sender.IsAlive() && GameState.IsInGamePlay && !GameState.IsMeeting && !GameState.IsExilling || DataManager.Settings.Multiplayer.ChatMode == InnerNet.QuickChatModes.QuickChatOnly)
        {
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatActionText()))
            {
                LogRpcInfo($"{sender.IsAlive()} && {GameState.IsInGamePlay} && {!GameState.IsMeeting} && {!GameState.IsExilling} || {DataManager.Settings.Multiplayer.ChatMode == InnerNet.QuickChatModes.QuickChatOnly}");
            }
        }
    }
}
