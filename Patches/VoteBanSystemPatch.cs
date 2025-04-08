using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
using BetterAmongUs.Modules;
using HarmonyLib;

namespace BetterAmongUs.Patches;

[HarmonyPatch(typeof(VoteBanSystem))]
internal static class VoteBanSystemPatch
{
    private static readonly Dictionary<VoteBanSystem, List<(int ClientId, (ushort HashPuid, string FriendCode) Voter)>> _voteData = [];

    [HarmonyPatch(nameof(VoteBanSystem.AddVote))]
    [HarmonyPrefix]
    private static bool AddVote_Prefix(VoteBanSystem __instance, int srcClient, int clientId)
    {
        if (!GameState.IsHost) return true;

        var client = Utils.ClientFromClientId(srcClient);
        if (client == null) return false;

        void TryFlagPlayer()
        {
            var player = client.Character;
            if (player != null)
            {
                BetterNotificationManager.NotifyCheat(player, string.Format(Translator.GetString("AntiCheat.InvalidLobbyRPC"), "VoteKick"));
            }
        }

        if (GameState.IsLobby)
        {
            TryFlagPlayer();
            return false;
        }

        if (!_voteData.TryGetValue(__instance, out var voters))
        {
            _voteData.Clear();
            _voteData[__instance] = voters = new List<(int, (ushort, string))>();
        }

        if (string.IsNullOrEmpty(client.ProductUserId) && string.IsNullOrEmpty(client.FriendCode))
        {
            return true;
        }

        var clientHash = Utils.GetHashUInt16(client.ProductUserId);

        foreach (var (targetClientId, (existingHash, existingFriendCode)) in voters)
        {
            if (targetClientId != clientId)
                continue;

            bool isDuplicateVote = existingHash == clientHash ||
                                  (!string.IsNullOrEmpty(client.FriendCode) &&
                                   existingFriendCode == client.FriendCode);

            if (isDuplicateVote)
            {
                return false;
            }
        }

        voters.Add((clientId, (clientHash, client.FriendCode)));
        return true;
    }
}