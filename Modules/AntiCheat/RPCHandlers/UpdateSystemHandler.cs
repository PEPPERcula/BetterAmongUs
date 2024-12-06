using BetterAmongUs.Helpers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

public class UpdateSystemHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.UpdateSystem;

    public SystemTypes CatchedSystemType;

    private readonly Dictionary<uint, Func<PlayerControl?, ISystemType, MessageReader, byte, bool>> systemHandlers;

    private static SabotageSystemType SabotageSystem => ShipStatus.Instance.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();

    public UpdateSystemHandler()
    {
        systemHandlers = new Dictionary<uint, Func<PlayerControl?, ISystemType, MessageReader, byte, bool>>
        {
            { (uint)SystemTypes.Sabotage, (sender, system, reader, count) => HandleSabotageSystem(sender, system.Cast<SabotageSystemType>(), reader) },
            { (uint)SystemTypes.Ventilation, (sender, system, reader, count) => HandleVentilationSystem(sender, system.Cast<VentilationSystem>(), count) },
            { (uint)SystemTypes.Electrical, (sender, system, reader, count) => HandleSwitchSystem(sender, system.Cast<SwitchSystem>(), count) },
            { (uint)SystemTypes.Comms, (sender, system, reader, count) => HandleCommsSystem(sender, system, count) },
            { (uint)SystemTypes.MushroomMixupSabotage, (sender, system, reader, count) => HandleMushroomMixupSabotageSystem(sender, system.Cast<MushroomMixupSabotageSystem>(), count) },
            { (uint)SystemTypes.Doors, (sender, system, reader, count) => HandleDoorsSystem(sender, system.Cast<DoorsSystemType>(), count) },
            { (uint)SystemTypes.Reactor, (sender, system, reader, count) => HandleReactorSystem(sender, system.Cast<ReactorSystemType>(), count) },
            { (uint)SystemTypes.Laboratory, (sender, system, reader, count) => HandleReactorSystem(sender, system.Cast<ReactorSystemType>(), count) },
            { (uint)SystemTypes.HeliSabotage, (sender, system, reader, count) => HandleHeliSabotageSystem(sender, system.Cast<HeliSabotageSystem>(), count) },
            { (uint)SystemTypes.LifeSupp, (sender, system, reader, count) => HandleLifeSuppSystem(sender, system.Cast<LifeSuppSystemType>(), count) }
        };
    }

    public override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        MessageReader oldReader = MessageReader.Get(reader);
        byte count = reader.ReadByte();

        Logger.InGame($"{Enum.GetName(CatchedSystemType)}");

        if (ShipStatus.Instance.Systems.TryGetValue(CatchedSystemType, out ISystemType system))
        {
            uint systemKey = (uint)CatchedSystemType;

            if (systemHandlers.TryGetValue(systemKey, out var handler))
            {
                return handler.Invoke(sender, system, oldReader, count);
            }
        }

        return true;
    }

    private static bool HandleSabotageSystem(PlayerControl? sender, SabotageSystemType sabotageSystem, MessageReader reader)
    {
        byte count = reader.ReadByte();

        if (!sender.IsImpostorTeam())
        {
            return false;
        }

        if (sabotageSystem.Timer > 0f)
        {
            return false;
        }

        return true;
    }

    private static bool HandleVentilationSystem(PlayerControl? sender, VentilationSystem ventilationSystem, byte count)
    {

        return true;
    }

    private static bool HandleSwitchSystem(PlayerControl? sender, SwitchSystem switchSystem, byte count)
    {
        if (count == 128) // Direct sabotage call from client, which is not possible, only the host should have this count when HandleSabotageSystem it's called
        {
            return false;
        }

        return true;
    }

    private static bool HandleCommsSystem(PlayerControl? sender, ISystemType system, byte count)
    {
        if (system == null) return false;

        if (system.TryCast<HqHudSystemType>(out var hqHudSystem))
        {
            return HandleHqHudSystem(sender, hqHudSystem, count);
        }
        if (system.TryCast<HudOverrideSystemType>(out var HudOverrideSystem))
        {
            return HandleHudOverrideSystem(sender, HudOverrideSystem, count);
        }

        return true;
    }

    private static bool HandleHqHudSystem(PlayerControl? sender, HqHudSystemType hqHudSystem, byte count)
    {
        if (count == 128) // Direct sabotage call from client, which is not possible, only the host should have this count when HandleSabotageSystem it's called
        {
            return false;
        }

        return true;
    }

    private static bool HandleHudOverrideSystem(PlayerControl? sender, HudOverrideSystemType hudOverrideSystem, byte count)
    {
        if (count == 128) // Direct sabotage call from client, which is not possible, only the host should have this count when HandleSabotageSystem it's called
        {
            return false;
        }

        return true;
    }

    private static bool HandleMushroomMixupSabotageSystem(PlayerControl? sender, MushroomMixupSabotageSystem mushroomMixupSabotage, byte count)
    {
        if (count == 1) // Direct sabotage call from client, which is not possible, only the host should have this count when HandleSabotageSystem it's called
        {
            return false;
        }

        return true;
    }

    private static bool HandleDoorsSystem(PlayerControl? sender, DoorsSystemType doorsSystem, byte count)
    {
        if (count == 128) // Direct sabotage call from client, which is not possible, only the host should have this count when HandleSabotageSystem it's called
        {
            return false;
        }

        return true;
    }

    private static bool HandleReactorSystem(PlayerControl? sender, ReactorSystemType reactorSystem, byte count)
    {
        if (count == 128) // Direct sabotage call from client, which is not possible, only the host should have this count when HandleSabotageSystem it's called
        {
            return false;
        }

        return true;
    }

    private static bool HandleHeliSabotageSystem(PlayerControl? sender, HeliSabotageSystem heliSabotageSystem, byte count)
    {
        if (count == 128) // Direct sabotage call from client, which is not possible, only the host should have this count when HandleSabotageSystem it's called
        {
            return false;
        }

        return true;
    }

    private static bool HandleLifeSuppSystem(PlayerControl? sender, LifeSuppSystemType lifeSuppSystem, byte count)
    {
        if (count == 128) // Direct sabotage call from client, which is not possible, only the host should have this count when HandleSabotageSystem it's called
        {
            return false;
        }

        return true;
    }
}