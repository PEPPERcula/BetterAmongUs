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
        return File.ReadLines(filePath)
                   .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("//"))
                   .SelectMany(line => line.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                           .Select(s => s.Trim()));
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
}
