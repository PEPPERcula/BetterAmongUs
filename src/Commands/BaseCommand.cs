using BetterAmongUs.Commands.Arguments;
using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Items.Enums;

namespace BetterAmongUs.Commands;

internal abstract class BaseCommand
{
    internal static readonly BaseCommand?[] allCommands = [.. RegisterCommandAttribute.Instances];
    internal virtual CommandType Type => CommandType.Normal;
    internal string[] Names => ShortNames.Concat(new[] { Name }).ToArray();
    internal abstract string Name { get; }
    internal virtual string[] ShortNames => [];
    internal abstract string Description { get; }
    internal BaseArgument[] Arguments { get; set; } = [];
    internal virtual bool SetChatTimer { get; set; } = false;
    internal virtual bool ShowCommand() => true;
    internal virtual bool ShowSuggestion() => ShowCommand();
    internal abstract void Run();

    internal static string CommandResultText(string text, bool onlyGetStr = false)
    {
        if (!onlyGetStr) Utils.AddChatPrivate(text);
        return text;
    }

    internal static string CommandErrorText(string error, bool onlyGetStr = false)
    {
        string er = "<color=#f50000><size=150%><b>Error:</b></size></color>";
        if (!onlyGetStr) Utils.AddChatPrivate($"<color=#730000>{er}\n{error}");
        return $"<color=#730000>{er}\n{error}";
    }
}
