using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Commands;

[RegisterCommand]
internal class TestCommand : BaseCommand
{
    internal override CommandType Type => CommandType.Debug;
    internal override string Name => "test";
    internal override string Description => "Test Command";
    internal override void Run()
    {
    }
}
