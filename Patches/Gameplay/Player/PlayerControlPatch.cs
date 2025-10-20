using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Enums;
using BetterAmongUs.Items.OptionItems;
using BetterAmongUs.Managers;
using HarmonyLib;

namespace BetterAmongUs.Patches.Gameplay.Player;

[HarmonyPatch(typeof(PlayerControl))]
internal static class PlayerControlPatch
{
    [HarmonyPatch(nameof(PlayerControl.Start))]
    [HarmonyPostfix]
    private static void Start_Postfix(PlayerControl __instance)
    {
        BAUPlugin.AllPlayerControls.Add(__instance);
        OptionPlayerItem.UpdateAllValues();
    }

    [HarmonyPatch(nameof(PlayerControl.OnDestroy))]
    [HarmonyPostfix]
    private static void OnDestroy_Postfix(PlayerControl __instance)
    {
        BAUPlugin.AllPlayerControls.Remove(__instance);
        OptionPlayerItem.UpdateAllValues();
    }

    [HarmonyPatch(nameof(PlayerControl.MurderPlayer))]
    [HarmonyPostfix]
    private static void MurderPlayer_Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        if (target == null) return;

        Logger.LogPrivate($"{__instance.Data.PlayerName} Has killed {target.Data.PlayerName} as {__instance.Data.RoleType.GetRoleName()}", "EventLog");

        __instance.BetterData().RoleInfo.Kills += 1;

        HostManager.SyncNames(NameSyncType.Gameplay);
    }

    [HarmonyPatch(nameof(PlayerControl.Shapeshift))]
    [HarmonyPostfix]
    private static void Shapeshift_Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target, [HarmonyArgument(1)] bool animate)
    {
        if (target == null) return;

        if (__instance != target)
            Logger.LogPrivate($"{__instance.Data.PlayerName} Has Shapeshifted into {target.Data.PlayerName}, did animate: {animate}", "EventLog");
        else
            Logger.LogPrivate($"{__instance.Data.PlayerName} Has Un-Shapeshifted, did animate: {animate}", "EventLog");

        HostManager.SyncNames(NameSyncType.Gameplay, 0.2f, 30);
    }

    [HarmonyPatch(nameof(PlayerControl.CompleteTask))]
    [HarmonyPostfix]
    private static void CompleteTask_Postfix(PlayerControl __instance)
    {
        HostManager.SyncNames(NameSyncType.Gameplay);
    }

    [HarmonyPatch(nameof(PlayerControl.StartMeeting))]
    [HarmonyPrefix]
    private static void StartMeeting_Prefix(PlayerControl __instance)
    {
        HostManager.SyncNames(NameSyncType.Meeting);
    }

    [HarmonyPatch(nameof(PlayerControl.SetRoleInvisibility))]
    [HarmonyPostfix]
    private static void SetRoleInvisibility_Postfix(PlayerControl __instance, [HarmonyArgument(0)] bool isActive, [HarmonyArgument(1)] bool animate)
    {

        if (isActive)
            Logger.LogPrivate($"{__instance.Data.PlayerName} Has Vanished as Phantom, did animate: {animate}", "EventLog");
        else
            Logger.LogPrivate($"{__instance.Data.PlayerName} Has Appeared as Phantom, did animate: {animate}", "EventLog");
    }
}

[HarmonyPatch(typeof(PlayerPhysics))]
internal static class PlayerPhysicsPatch
{
    [HarmonyPatch(nameof(PlayerPhysics.BootFromVent))]
    [HarmonyPostfix]
    private static void BootFromVent_Postfix(PlayerPhysics __instance, [HarmonyArgument(0)] int ventId)
    {

        Logger.LogPrivate($"{__instance.myPlayer.Data.PlayerName} Has been booted from vent: {ventId}, as {__instance.myPlayer.Data.RoleType.GetRoleName()}", "EventLog");
    }

    [HarmonyPatch(nameof(PlayerPhysics.CoEnterVent))]
    [HarmonyPostfix]
    private static void CoEnterVent_Postfix(PlayerPhysics __instance, [HarmonyArgument(0)] int ventId)
    {

        Logger.LogPrivate($"{__instance.myPlayer.Data.PlayerName} Has entered vent: {ventId}, as {__instance.myPlayer.Data.RoleType.GetRoleName()}", "EventLog");
    }

    [HarmonyPatch(nameof(PlayerPhysics.CoExitVent))]
    [HarmonyPostfix]
    private static void CoExitVent_Postfix(PlayerPhysics __instance, [HarmonyArgument(0)] int ventId)
    {

        Logger.LogPrivate($"{__instance.myPlayer.Data.PlayerName} Has exit vent: {ventId}, as {__instance.myPlayer.Data.RoleType.GetRoleName()}", "EventLog");
    }
}
