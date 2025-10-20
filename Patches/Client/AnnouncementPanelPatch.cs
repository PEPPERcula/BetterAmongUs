using AmongUs.Data.Player;
using Assets.InnerNet;
using BetterAmongUs.Helpers;
using BetterAmongUs.Items;
using BetterAmongUs.Items.Enums;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace BetterAmongUs.Patches.Client;

[HarmonyPatch]
internal static class AnnouncementPanelPatch
{
    [HarmonyPatch(typeof(PlayerAnnouncementData), nameof(PlayerAnnouncementData.SetAnnouncements)), HarmonyPrefix]
    private static bool SetModAnnouncements(PlayerAnnouncementData __instance, [HarmonyArgument(0)] ref Il2CppReferenceArray<Announcement> aRange)
    {
        ModNews.ProcessModNewsFiles();

        ModNews.AllModNews.Sort((a1, a2) => DateTime.Compare(DateTime.Parse(a2.Date), DateTime.Parse(a1.Date)));

        var finalAllNews = ModNews.AllModNews.Select(n => n.ToAnnouncement()).ToList();

        foreach (var news in aRange)
        {
            if (!ModNews.AllModNews.Any(x => x.Number == news.Number))
            {
                finalAllNews.Add(news);
            }
        }

        finalAllNews.Sort((a1, a2) => DateTime.Compare(DateTime.Parse(a2.Date), DateTime.Parse(a1.Date)));

        aRange = new Il2CppReferenceArray<Announcement>(finalAllNews.Count);
        for (int i = 0; i < finalAllNews.Count; i++)
        {
            aRange[i] = finalAllNews[i];
        }

        return true;
    }

    [HarmonyPatch(typeof(AnnouncementPanel), nameof(AnnouncementPanel.SetUp)), HarmonyPostfix]
    private static void SetUpPanel(AnnouncementPanel __instance, [HarmonyArgument(0)] Announcement announcement)
    {
        if (announcement.Number >= 100000)
        {
            var obj = new GameObject("ModLabel");
            obj.transform.SetParent(__instance.transform);
            obj.transform.localPosition = new Vector3(-0.8f, 0.13f, 0.5f);
            obj.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);

            var renderer = obj.AddComponent<SpriteRenderer>();
            var modNews = ModNews.AllModNews.Find(a => a.Number == announcement.Number);

            if (modNews != null)
            {
                switch (modNews.NewsType)
                {
                    case NewsTypes.BAU:
                        renderer.sprite = Utils.LoadSprite("BetterAmongUs.Resources.Images.BetterAmongUs-Icon.png", 1225f);
                        break;
                    case NewsTypes.TEN:
                        renderer.sprite = Utils.LoadSprite("BetterAmongUs.Resources.Images.TEN_Icon.png", 1500f);
                        break;
                }

                renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            }
        }
    }
}