using System.Text.Json;

namespace BetterAmongUs;

public class BetterAccountInfo
{
    public string? HashPUID = "";
    public string? FriendCode = "";
    public string? OverHeadTag = "";
    public string? OverHeadColor = "";
    public bool? IsDev = false;
    public string? DevType = "none";
    public bool? IsSponsor = false;
    public int? SponsorTier = 0;

    public static BetterAccountInfo GenerateInfo(string allUserInfo, NetworkedPlayerInfo data)
    {
        BetterAccountInfo accInfo = new();

        try
        {
            var userList = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(allUserInfo);
            var userInfo = userList?.FirstOrDefault(u => u.TryGetValue("friendcode", out var friendCode) && friendCode.ToString() == data.FriendCode ||
                                                           u.TryGetValue("puid", out var puid) && puid.ToString() == data.FriendCode);

            if (userInfo != null && userInfo.Count > 0)
            {
                accInfo.HashPUID = Utils.GetHashPuid(data.Puid);
                accInfo.FriendCode = data.FriendCode;

                if (userInfo.TryGetValue("overhead_tag", out var tag))
                    accInfo.OverHeadTag = tag.ToString();

                if (userInfo.TryGetValue("overhead_color", out var color))
                    accInfo.OverHeadColor = color.ToString();

                if (userInfo.TryGetValue("is_dev", out var isDev) && bool.TryParse(isDev.ToString(), out bool dev))
                    accInfo.IsDev = dev;

                if (userInfo.TryGetValue("dev_type", out var type))
                    accInfo.DevType = type.ToString();

                if (userInfo.TryGetValue("is_sponsor", out var isSponsor) && bool.TryParse(isSponsor.ToString(), out bool sponsor))
                    accInfo.IsSponsor = sponsor;

                if (userInfo.TryGetValue("sponsor_tier", out var tier) && int.TryParse(tier.ToString(), out int sponsorTier))
                    accInfo.SponsorTier = sponsorTier;
            }
        }
        catch (JsonException ex)
        {
            Logger.Error($"JSON error: {ex.Message}");
        }

        return accInfo;
    }
}
