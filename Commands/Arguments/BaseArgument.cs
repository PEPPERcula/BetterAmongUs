namespace BetterAmongUs.Commands
{
    internal abstract class BaseArgument(BaseCommand? command, string argInfo)
    {
        internal BaseCommand? Command { get; } = command;
        internal string ArgInfo { get; } = argInfo;
        internal string Arg { get; set; } = string.Empty;
        protected virtual string[] ArgSuggestions => GetArgSuggestions.Invoke();
        internal Func<string[]> GetArgSuggestions { get; set; } = () => { return []; };
        internal string GetClosestSuggestion() => ArgSuggestions.FirstOrDefault(name => name.StartsWith(Arg, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
    }
}
