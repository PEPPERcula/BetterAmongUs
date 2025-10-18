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
            var separatorIndex = line.IndexOf(':');
            if (separatorIndex > -1)
            {
                if (currentKey != null && currentValue.Length > 0)
                {
                    result[currentKey] = currentValue.ToString().Trim();
                    currentValue.Clear();
                }

                var key = line[..separatorIndex].Trim();
                var value = line[(separatorIndex + 1)..].Trim();

                currentKey = key;
                currentValue.Append(value);
            }
            else if (currentKey != null && line.Trim().Length > 0)
            {
                currentValue.Append('\n').Append(line.Trim());
            }
        }

        if (currentKey != null && currentValue.Length > 0)
        {
            result[currentKey] = currentValue.ToString().Trim();
        }

        return result;
    }
}
