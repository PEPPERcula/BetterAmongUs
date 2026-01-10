using AmongUs.Data;
using Assets.InnerNet;
using BetterAmongUs.Enums;
using BetterAmongUs.Helpers;
using BetterAmongUs.Network.Configs;
using UnityEngine;

namespace BetterAmongUs.Modules;

internal sealed class ModNews
{
    internal NewsTypes NewsType { get; }
    internal int Number { get; }
    internal string Title { get; }
    internal string SubTitle { get; }
    internal string ShortTitle { get; }
    public Dictionary<int, string> Contents { get; set; } = [];
    internal string Date { get; }

    internal static List<NewsData> NewsDataToProcess { get; } = [];
    internal static List<ModNews> AllModNews { get; } = [];

    internal ModNews(NewsTypes type, int number, string title, string subTitle, string shortTitle, Dictionary<int, string> contents, string date)
    {
        NewsType = type;
        Number = number;
        Title = title;
        SubTitle = subTitle;
        ShortTitle = shortTitle;
        Contents = contents;
        Date = date;

        AllModNews.Add(this);
    }

    internal Announcement ToAnnouncement()
    {
        var announcement = new Announcement
        {
            Number = Number,
            Title = Title,
            SubTitle = SubTitle,
            ShortTitle = ShortTitle,
            Text = "Error processing translation!".ToColor(Color.red),
            Language = (uint)DataManager.Settings.Language.CurrentLanguage,
            Date = Date,
            Id = "ModNews"
        };

        if (Contents.TryGetValue((int)Translator.GetTargetLanguageId(), out var content))
        {
            announcement.Text = content;
        }
        else if (Contents.TryGetValue((int)SupportedLangs.English, out var englishContent))
        {
            announcement.Text = englishContent;
        }

        return announcement;
    }

    private static int _nextAnnouncementNumber;
    internal static void ProcessModNewsFiles()
    {
        AllModNews.Clear();

        _nextAnnouncementNumber = 110000;
        foreach (var config in NewsDataToProcess)
        {
            ParseModNewsContent(config, _nextAnnouncementNumber);
            _nextAnnouncementNumber++;
        }
    }

    private static void ParseModNewsContent(NewsData config, int nextAnnouncementNumber)
    {
        var type = (NewsTypes)config.Type;
        _ = new ModNews(type, nextAnnouncementNumber, config.Title, config.SubTitle, config.ListTitle, config.Contents, config.Date);
    }
}