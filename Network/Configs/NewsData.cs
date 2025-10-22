using BetterAmongUs.Modules;

namespace BetterAmongUs.Network.Configs;

/// <summary>
/// Represents the data structure for mod news items within the game.
/// Contains properties to store news-related details such as title, subtitle, content, and metadata.
/// </summary>
internal class NewsData()
{
    /// <summary>
    /// Indicates whether the news item should be shown.
    /// </summary>
    public bool Show { get; set; }

    /// <summary>
    /// Defines the type/category of the news item.
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// Unique identifier for the news item.
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    /// The main title of the news item.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The subtitle providing additional context to the news title.
    /// </summary>
    public string SubTitle { get; set; } = string.Empty;

    /// <summary>
    /// The title used for listing purposes.
    /// </summary>
    public string ListTitle { get; set; } = string.Empty;

    /// <summary>
    /// The publication date of the news item, formatted as a string.
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// The content or body of the news item.
    /// </summary>
    public Dictionary<int, string> Contents { get; set; } = [];

    internal static NewsData? Serialize(string input)
    {
        var newsData = new NewsData();

        try
        {
            var data = TextFileHandler.ParseYaml(input);

            if (data.TryGetValue("show", out var show))
                newsData.Show = bool.Parse(show);
            if (data.TryGetValue("type", out var type))
                newsData.Type = int.Parse(type);
            if (data.TryGetValue("id", out var id))
                newsData.Id = uint.Parse(id);
            if (data.TryGetValue("title", out var title))
                newsData.Title = title;
            if (data.TryGetValue("subtitle", out var subtitle))
                newsData.SubTitle = subtitle;
            if (data.TryGetValue("listtitle", out var listtitle))
                newsData.ListTitle = listtitle;
            if (data.TryGetValue("date", out var date))
                newsData.Date = date;

            foreach (var kvp in Translator.TranslateIdLookup)
            {
                if (data.TryGetValue($"content-{kvp.Key}", out var content))
                {
                    newsData.Contents[kvp.Value] = TextFileHandler.FormatToRichText(content.Replace("|", ""));
                }
            }

            return newsData;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to manually deserialize YAML: {ex.Message}");
            return null;
        }
    }
}