using AmongUs.Data;
using Assets.InnerNet;
using BetterAmongUs.Items.Enums;
using BetterAmongUs.Network.Configs;

namespace BetterAmongUs.Items;

internal class ModNews
{
    internal NewsTypes NewsType { get; }
    internal int Number { get; }
    internal string Title { get; }
    internal string SubTitle { get; }
    internal string ShortTitle { get; }
    internal string Text { get; }
    internal string Date { get; }

    internal static List<NewsData> NewsDataToProcess { get; } = new();
    internal static List<ModNews> AllModNews { get; } = new();

    internal ModNews(NewsTypes type, int number, string title, string subTitle, string shortTitle, string text, string date)
    {
        NewsType = type;
        Number = number;
        Title = title;
        SubTitle = subTitle;
        ShortTitle = shortTitle;
        Text = text;
        Date = date;

        AllModNews.Add(this);
    }

    internal Announcement ToAnnouncement()
    {
        return new Announcement
        {
            Number = Number,
            Title = Title,
            SubTitle = SubTitle,
            ShortTitle = ShortTitle,
            Text = Text,
            Language = (uint)DataManager.Settings.Language.CurrentLanguage,
            Date = Date,
            Id = "ModNews"
        };
    }

    internal static void ProcessModNewsFiles()
    {
        AllModNews.Clear();

        foreach (var config in NewsDataToProcess)
        {
            ParseModNewsContent(config);
        }
    }

    private static void ParseModNewsContent(NewsData config)
    {
        if (config.Id == 0) return;

        var type = (NewsTypes)config.Type;
        _ = new ModNews(type, (int)config.Id, config.Title, config.SubTitle, config.ListTitle, config.Content, config.Date);
    }
}