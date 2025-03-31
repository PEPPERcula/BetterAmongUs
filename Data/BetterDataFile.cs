using BetterAmongUs.Items.Structs;
using System.Text.Json.Serialization;

namespace BetterAmongUs.Data;

internal sealed class BetterDataFile : AbstractJsonFile<BetterDataFile>
{
    [JsonIgnore]
    internal override string FilePath => Path.Combine(Main.GetDataPathToAmongUs(), $"BetterDataV2.json");

    protected override bool Load()
    {
        var success = base.Load();
        if (success)
        {
            AllCheatData = [.. CheatData, .. SickoData, .. AUMData, .. KNData];
        }
        return success;
    }

    internal override bool Save()
    {
        AllCheatData = [.. CheatData, .. SickoData, .. AUMData, .. KNData];
        return base.Save();
    }

    internal bool CheckPlayerData(NetworkedPlayerInfo data) => CheckPlayerDataWithReason(data).check;

    internal (bool check, string reason) CheckPlayerDataWithReason(NetworkedPlayerInfo data)
    {
        foreach (var info in AllCheatData)
        {
            var (check, reason) = info.CheckPlayerDataWithReason(data);
            if (check)
            {
                return (true, reason);
            }
        }

        return (false, "");
    }

    [JsonIgnore]
    internal HashSet<UserInfo> AllCheatData { get; set; } = [];

    [JsonPropertyName("cheatData")]
    public HashSet<UserInfo> CheatData { get; set; } = [];

    [JsonPropertyName("sickoData")]
    public HashSet<UserInfo> SickoData { get; set; } = [];

    [JsonPropertyName("aumData")]
    public HashSet<UserInfo> AUMData { get; set; } = [];

    [JsonPropertyName("knData")]
    public HashSet<UserInfo> KNData { get; set; } = [];
}