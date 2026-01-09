using BetterAmongUs.Attributes;
using BetterAmongUs.Managers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class PetHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.Pet;
    internal override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        if (sender?.CurrentOutfit?.PetId == "pet_EmptyPet")
        {
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatActionText()))
            {
                LogRpcInfo($"{sender?.CurrentOutfit?.PetId == "pet_EmptyPet"}");
            }
        }
    }
}
