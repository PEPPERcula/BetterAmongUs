using AmongUs.Data;
using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
using BetterAmongUs.Patches;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

public class SendChatHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SendChat;

    public override void Handle(PlayerControl? sender, MessageReader reader)
    {
        var text = reader.ReadString();

        // Check banned words
        if (BetterGameSettings.UseBanWordList.GetBool())
        {
            try
            {
                Func<string, string> normalizeText = text => new string(text.Where(c => char.IsLetterOrDigit(c)).ToArray()).ToLower();

                HashSet<string> bannedWords = new HashSet<string>(
                    File.ReadLines(BetterDataManager.banWordListFile)
                        .Where(line => !line.TrimStart().StartsWith("//"))
                        .Select(normalizeText)
                        .Where(text => !string.IsNullOrWhiteSpace(text))
                );

                string normalizedMessage = normalizeText(text);

                bool isWordBanned = bannedWords.Any(bannedWord =>
                    normalizedMessage.Contains(bannedWord)
                );

                if (!string.IsNullOrEmpty(normalizedMessage) && isWordBanned)
                {
                    _ = new LateTask(() =>
                    {
                        sender.Kick(false, $"has been kicked due to\nchat message containing a banned word!");
                    }, 1f, shoudLog: false);
                }
            }
            catch { }
        }
    }

    public override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        if (sender.IsAlive() && GameState.IsInGamePlay && !GameState.IsMeeting && !GameState.IsExilling || DataManager.Settings.Multiplayer.ChatMode == InnerNet.QuickChatModes.QuickChatOnly)
        {
            BetterNotificationManager.NotifyCheat(sender, GetFormatActionText());
            LogRpcInfo($"{sender.IsAlive()} && {GameState.IsInGamePlay} && {!GameState.IsMeeting} && {!GameState.IsExilling} || {DataManager.Settings.Multiplayer.ChatMode == InnerNet.QuickChatModes.QuickChatOnly}");
        }
    }
}
