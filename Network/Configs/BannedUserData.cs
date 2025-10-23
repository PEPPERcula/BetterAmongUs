using BetterAmongUs.Helpers;
using System.Text.Json.Serialization;

namespace BetterAmongUs.Network.Configs;

[method: JsonConstructor]
internal class BannedUserData(string name = "", string puid = "", string friendCode = "", string reason = "")
{
    internal static List<BannedUserData> AllBannedUsers = [new BannedUserData("Default")];

    internal bool IsLocalBan { get; private set; }

    [JsonPropertyName("name")]
    internal string Name { get; } = name;

    [JsonPropertyName("puid")]
    internal string Puid { get; } = puid;

    [JsonPropertyName("friendcode")]
    internal string FriendCode { get; } = friendCode;

    [JsonPropertyName("reason")]
    internal string Reason { get; } = reason;

    internal static bool IsBanned = false;
    internal static bool CheckLocalBan(out BannedUserData? bannedData)
    {
        bannedData = null;
        if (IsBanned) return true;

        if (EOSManager.Instance)
        {
            var data = AllBannedUsers?.FirstOrDefault(user => user.Puid == EOSManager.Instance.ProductUserId.GetHashStr() || user.FriendCode == EOSManager.Instance.FriendCode.GetHashStr());
            if (data != null)
            {
                bannedData = data;
                data.IsLocalBan = true;
                IsBanned = true;
                return true;
            }
        }

        return false;
    }
    internal static bool CheckPlayerBan(NetworkedPlayerInfo data) => AllBannedUsers?.FirstOrDefault(user => user.Puid == data.GetHashPuid() || user.FriendCode == data.GetHashFriendcode()) != null;
    internal static bool CheckPuidBan(string puid) => AllBannedUsers?.FirstOrDefault(user => user.Puid == puid.GetHashStr()) != null;
    internal static bool CheckFriendCodeBan(string friendcode) => AllBannedUsers?.FirstOrDefault(user => user.FriendCode == friendcode.GetHashStr()) != null;
}