using BetterAmongUs.Helpers;
using System.Text.Json.Serialization;

namespace BetterAmongUs.Items;

[Flags]
internal enum MultiPermissionFlags : ushort
{
    Contributor_1 = 1 << 0,
    Contributor_2 = 1 << 1,
    Contributor_3 = 1 << 2,
    Tester = 1 << 3,
    Staff = 1 << 4,
    Dev = 1 << 5,
    All = 1 << 6
}

[method: JsonConstructor]
internal class UserData(string name = "", string puid = "", string friendCode = "", string overheadTag = "", string overheadColor = "", ushort permissions = 0)
{
    internal static List<UserData> AllUsers = [new UserData("Default")];

    internal bool IsLocalData { get; private set; }

    [JsonPropertyName("name")]
    internal string Name { get; } = name;

    [JsonPropertyName("puid")]
    internal string Puid { get; } = puid;

    [JsonPropertyName("friendcode")]
    internal string FriendCode { get; } = friendCode;

    [JsonPropertyName("overheadtag")]
    internal string OverheadTag { get; } = overheadTag;

    [JsonPropertyName("overheadColor")]
    internal string OverheadColor { get; } = overheadColor;

    [JsonPropertyName("permissions")]
    internal ushort Permissions { get; } = permissions;

    private static bool HasLocalData = false;
    internal static void TrySetLocalData()
    {
        if (!HasLocalData)
        {
            if (EOSManager.Instance)
            {
                var data = AllUsers?.FirstOrDefault(user => user.Puid == Utils.GetHashStr(EOSManager.Instance.ProductUserId) || user.FriendCode == Utils.GetHashStr(EOSManager.Instance.FriendCode));
                if (data != null)
                {
                    Logger.Log($"Found local UserData({data.Name})");
                    data.IsLocalData = true;
                    BAUPlugin.MyData = new(data.Name, data.Puid, data.FriendCode, data.OverheadTag, data.OverheadColor, data.Permissions)
                    {
                        IsLocalData = true
                    };
                    HasLocalData = true;
                }
            }
        }
    }
    internal static UserData? GetPlayerUserData(NetworkedPlayerInfo data) => AllUsers?.FirstOrDefault(user => user.Puid == data.GetHashPuid() || user.FriendCode == data.GetHashFriendcode()) ?? AllUsers.First();
    internal static UserData? GetPlayerUserDataFromPuid(string puid) => AllUsers?.FirstOrDefault(user => user.Puid == Utils.GetHashStr(puid)) ?? AllUsers.First();
    internal static UserData? GetPlayerUserDataFromFriendCode(string friendcode) => AllUsers?.FirstOrDefault(user => user.FriendCode == Utils.GetHashStr(friendcode)) ?? AllUsers.First();

    private bool HasPermission(MultiPermissionFlags permissionFlags)
    {
        if ((Permissions & (ushort)MultiPermissionFlags.All) == (ushort)MultiPermissionFlags.All)
        {
            return true;
        }

        return (Permissions & (ushort)permissionFlags) == (ushort)permissionFlags;
    }

    internal bool IsDev() => HasPermission(MultiPermissionFlags.Dev);
    internal bool IsTester() => HasPermission(MultiPermissionFlags.Tester);
    internal bool IsSponsorTier3() => HasPermission(MultiPermissionFlags.Contributor_3);
    internal bool IsSponsorTier2() => HasPermission(MultiPermissionFlags.Contributor_2 | MultiPermissionFlags.Contributor_3);
    internal bool IsSponsorTier1() => IsSponsor();
    internal bool IsSponsor() => HasPermission(MultiPermissionFlags.Contributor_1 | MultiPermissionFlags.Contributor_2 | MultiPermissionFlags.Contributor_3 | MultiPermissionFlags.Dev);
    internal bool HasAll() => (Permissions & (ushort)MultiPermissionFlags.All) == (ushort)MultiPermissionFlags.All;
    internal bool IsVerified() => Puid == Utils.GetHashStr(EOSManager.Instance.ProductUserId) && FriendCode == Utils.GetHashStr(EOSManager.Instance.FriendCode);
    internal bool IsVerified(NetworkedPlayerInfo data) => Puid == Utils.GetHashStr(data?.Puid ?? string.Empty) && FriendCode == Utils.GetHashStr(data?.FriendCode ?? string.Empty);
    internal bool IsVerified(PlayerControl player) => Puid == Utils.GetHashStr(player?.Data?.Puid ?? string.Empty) && FriendCode == Utils.GetHashStr(player?.Data?.FriendCode ?? string.Empty);
}