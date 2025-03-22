using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Commands;

public enum CommandType
{
    Normal,
    Sponsor,
    Debug,
}

public abstract class BaseCommand
{
    public static readonly BaseCommand?[] allCommands = [.. RegisterCommandAttribute.Instances];
    public virtual CommandType Type => CommandType.Normal;
    public string[] Names => ShortNames.Concat(new[] { Name }).ToArray();
    public abstract string Name { get; }
    public virtual string[] ShortNames => [];
    public abstract string Description { get; }
    public virtual BaseArgument[]? Arguments { get; } = [];
    public virtual bool SetChatTimer { get; set; } = false;
    public virtual bool ShowCommand() => true;
    public virtual bool ShowSuggestion() => ShowCommand();
    public abstract void Run();

    public static string CommandResultText(string text, bool onlyGetStr = false)
    {
        if (!onlyGetStr) Utils.AddChatPrivate(text);
        return text;
    }

    public static string CommandErrorText(string error, bool onlyGetStr = false)
    {
        string er = "<color=#f50000><size=150%><b>Error:</b></size></color>";
        if (!onlyGetStr) Utils.AddChatPrivate($"<color=#730000>{er}\n{error}");
        return $"<color=#730000>{er}\n{error}";
    }
}
