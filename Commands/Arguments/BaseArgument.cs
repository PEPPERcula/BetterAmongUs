namespace BetterAmongUs.Commands
{
    public abstract class BaseArgument(BaseCommand? command, string argInfo)
    {
        public BaseCommand? Command { get; } = command;
        public string ArgInfo { get; } = argInfo;
        public string Arg { get; set; } = string.Empty;
        protected virtual string[] ArgSuggestions => GetArgSuggestions.Invoke();
        public Func<string[]> GetArgSuggestions { get; set; } = () => { return []; };
        public string GetClosestSuggestion() => ArgSuggestions.FirstOrDefault(name => name.StartsWith(Arg, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
    }
}
