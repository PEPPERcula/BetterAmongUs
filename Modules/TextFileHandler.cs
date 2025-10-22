using System.Text;
using System.Text.RegularExpressions;

namespace BetterAmongUs.Modules;

internal static class TextFileHandler
{
    internal static bool CompareStringFilters(string filePath, string[] strings)
    {
        foreach (var content in ReadContents(filePath))
        {
            foreach (var text in strings)
            {
                if (CheckFilterString(content, text))
                {
                    return true;
                }
            }
        }

        return false;
    }

    internal static bool CompareStringMatch(string filePath, string[] strings)
    {
        var stringSet = new HashSet<string>(
            strings.Select(s => s.ToLower().Trim()),
            StringComparer.OrdinalIgnoreCase
        );

        foreach (var content in ReadContents(filePath))
        {
            string normalizedContent = content.ToLower().Trim();

            if (stringSet.Contains(normalizedContent))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<string> ReadContents(string filePath)
    {
        if (File.Exists(filePath))
        {
            return File.ReadLines(filePath)
                       .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("//"))
                       .SelectMany(line => line.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                               .Select(s => s.Trim()));
        }

        return [];
    }

    private static bool CheckFilterString(string filter, string text)
    {
        string pattern = filter switch
        {
            _ when filter.StartsWith("**") && filter.EndsWith("**") => Regex.Escape(filter.Trim('*')), // Contains anywhere
            _ when filter.StartsWith("**") => Regex.Escape(filter.TrimStart('*')) + "$", // Ends with
            _ when filter.EndsWith("**") => "^" + Regex.Escape(filter.TrimEnd('*')), // Starts with
            _ => "^" + Regex.Escape(filter) + "$" // Exact match
        };

        return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    internal static Dictionary<string, string> ParseYaml(string input)
    {
        var result = new Dictionary<string, string>();
        var lines = input.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

        string? currentKey = null;
        StringBuilder currentValue = new();

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Skip empty lines
            if (string.IsNullOrEmpty(trimmedLine))
                continue;

            var separatorIndex = line.IndexOf(':');

            // Check if this is a key-value line (has colon and is not in the middle of content)
            if (separatorIndex > -1 && IsKeyValueLine(line, separatorIndex))
            {
                // Save previous key-value pair if it exists
                if (currentKey != null && currentValue.Length > 0)
                {
                    result[currentKey] = currentValue.ToString().Trim();
                    currentValue.Clear();
                }

                var key = line[..separatorIndex].Trim();
                var value = line[(separatorIndex + 1)..].Trim();

                currentKey = key;
                if (!string.IsNullOrEmpty(value))
                {
                    currentValue.Append(value);
                }
            }
            else if (currentKey != null)
            {
                // This is a continuation line for the current value
                if (currentValue.Length > 0)
                    currentValue.Append('\n');
                currentValue.Append(line.Trim());
            }
        }

        if (currentKey != null && currentValue.Length > 0)
        {
            result[currentKey] = currentValue.ToString().Trim();
        }

        return result;
    }

    private static bool IsKeyValueLine(string line, int separatorIndex)
    {
        string keyPart = line[..separatorIndex].Trim();
        if (keyPart.Contains(' ') || keyPart.Contains('#') || keyPart.Contains('*') || keyPart.Contains('[') || keyPart.Contains('`'))
            return false;
        return true;
    }

    internal static string FormatToRichText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Process different elements in order of precedence
        text = ProcessHeaders(text);
        text = ProcessBlockQuotes(text);
        text = ProcessHorizontalRules(text);
        text = ProcessBoldAndItalic(text);
        text = ProcessStrikethrough(text);
        text = ProcessLinks(text);
        text = ProcessInlineCode(text);
        text = ProcessLineBreaks(text);

        return text;
    }

    private static string ProcessHeaders(string text)
    {
        // H1 to H6 with decreasing sizes
        var headerSizes = new Dictionary<int, int>
        {
            {1, 200}, {2, 180}, {3, 160}, {4, 140}, {5, 120}, {6, 110}
        };

        foreach (var header in headerSizes)
        {
            string pattern = @"^(#{1," + header.Key + @"})\s+(.+?)(?=\n|$)";
            string replacement = $"<size={header.Value}%><b>$2</b></size>\n";
            text = Regex.Replace(text, pattern, replacement, RegexOptions.Multiline);
        }

        return text;
    }

    private static string ProcessBlockQuotes(string text)
    {
        // Process > block quotes
        return Regex.Replace(text, @"^>\s+(.+?)(?=\n|$)", "<color=#888888><i>│ $1</i></color>", RegexOptions.Multiline);
    }

    private static string ProcessHorizontalRules(string text)
    {
        // Process --- or *** horizontal rules
        return Regex.Replace(text, @"^\s*([-*_]){3,}\s*$", "────────────────────", RegexOptions.Multiline);
    }

    private static string ProcessBoldAndItalic(string text)
    {
        // Bold: **text** or __text__
        text = Regex.Replace(text, @"(\*\*|__)(?![*\s])(.*?)(?<![*\s])\1", "<b>$2</b>");

        // Italic: *text* or _text_
        text = Regex.Replace(text, @"(\*|_)(?![*\s])(.*?)(?<![*\s])\1", "<i>$2</i>");

        // Bold + Italic: ***text*** or ___text___
        text = Regex.Replace(text, @"(\*\*\*|___)(?![*\s])(.*?)(?<![*\s])\1", "<b><i>$2</i></b>");

        return text;
    }

    private static string ProcessStrikethrough(string text)
    {
        // Strikethrough: ~~text~~
        return Regex.Replace(text, @"~~(.+?)~~", "<s>$1</s>");
    }

    private static string ProcessLinks(string text)
    {
        // Simple pattern that just matches the link itself
        return Regex.Replace(text, @"\[([^\]]+)\]\(([^)]+)\)",
            "<link=\"$2\"> <b>$1</b></link> ");
    }

    private static string ProcessInlineCode(string text)
    {
        // Inline code with monospace-like appearance
        return Regex.Replace(text, @"`([^`]+)`", "<color=#FF8C00><size=85%>$1</size></color>");
    }

    private static string ProcessLineBreaks(string text)
    {
        // Convert double newlines to proper paragraph breaks
        return Regex.Replace(text, @"\n\s*\n", "\n\n");
    }
}
