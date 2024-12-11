using BetterAmongUs.Managers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

public class PetHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.Pet;
    public override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        if (sender?.CurrentOutfit?.PetId == "pet_EmptyPet")
        {
            BetterNotificationManager.NotifyCheat(sender, GetFormatActionText());
            LogRpcInfo($"{sender?.CurrentOutfit?.PetId == "pet_EmptyPet"}");
        }
    }
}
