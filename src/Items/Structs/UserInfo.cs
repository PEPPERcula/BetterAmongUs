using BetterAmongUs.Helpers;
using System.Text.Json.Serialization;

namespace BetterAmongUs.Items.Structs;

[method: JsonConstructor]
internal sealed class UserInfo(string playerName, string hashPuid, string friendCode, string reason)
{
    internal bool CheckPlayerData(NetworkedPlayerInfo data) => CheckPlayerDataWithReason(data).check;

    internal (bool check, string reason) CheckPlayerDataWithReason(NetworkedPlayerInfo data)
    {
        if ((!string.IsNullOrEmpty(data.GetHashPuid()) && HashPuid == data.GetHashPuid())
            || (!string.IsNullOrEmpty(data.FriendCode) && FriendCode == data.FriendCode))
        {
            return (true, Reason);
        }
        else if (!string.IsNullOrEmpty(data.PlayerName) && PlayerName == data.PlayerName)
        {
            return (true, Reason);
        }

        return (false, "");
    }

    [JsonPropertyName("playerName")]
    public string PlayerName { get; set; } = playerName;

    [JsonPropertyName("hashPuid")]
    public string HashPuid { get; set; } = hashPuid;

    [JsonPropertyName("friendCode")]
    public string FriendCode { get; set; } = friendCode;

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = reason;
}