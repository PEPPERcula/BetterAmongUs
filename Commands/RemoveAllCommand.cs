using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;

namespace BetterAmongUs.Commands;

public class RemoveAllCommand : BaseCommand
{
    public override string Name => "removeall";
    public override string Description => "Remove all players from local <color=#4f92ff>Anti-Cheat</color> data";
    public override void Run()
    {
        BetterDataManager.ClearCheatData();
        Utils.AddChatPrivate($"All data successfully removed from local <color=#4f92ff>Anti-Cheat</color>!");
        Utils.DirtyAllNames();
    }
}
