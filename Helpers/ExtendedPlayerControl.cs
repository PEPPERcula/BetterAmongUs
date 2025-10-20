using BepInEx.Unity.IL2CPP.Utils;
using BetterAmongUs.Modules;
using BetterAmongUs.Mono;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using System.Collections;
using UnityEngine;

namespace BetterAmongUs.Helpers;

internal class ExtendedPlayerControl : MonoBehaviour, IMonoExtension<PlayerControl>
{
    public PlayerControl? BaseMono { get; set; }
    internal PlayerControl? _Player => BaseMono;

    private void Awake()
    {
        if (!MonoExtensionManager.RegisterExtension(this)) return;
        this.StartCoroutine(CoAddBetterData());
        _Player.gameObject.AddComponent<PlayerInfoDisplay>().Init(_Player);
    }

    [HideFromIl2Cpp]
    private IEnumerator CoAddBetterData()
    {
        while (_Player?.Data == null)
        {
            yield return null;
        }

        TryCreateExtendedData(_Player.Data);
    }

    internal static void TryCreateExtendedData(NetworkedPlayerInfo data)
    {
        if (data.BetterData() == null)
        {
            ExtendedPlayerInfo newBetterData = data.gameObject.AddComponent<ExtendedPlayerInfo>();
            newBetterData.SetInfo(data);
        }
    }

    private void OnDestroy()
    {
        MonoExtensionManager.UnregisterExtension(this);
    }

    internal readonly Dictionary<NetworkedPlayerInfo, string> NameSetLastFor = [];
}

internal static class PlayerControlExtension
{
    [HarmonyPatch(typeof(PlayerControl))]
    class PlayerControlPatch
    {
        [HarmonyPatch(nameof(PlayerControl.Awake))]
        [HarmonyPrefix]
        internal static void Awake_Prefix(PlayerControl __instance)
        {
            TryCreateExtendedPlayerControl(__instance);
        }

        internal static void TryCreateExtendedPlayerControl(PlayerControl pc)
        {
            if (pc.BetterPlayerControl() == null)
            {
                ExtendedPlayerControl newExtendedPc = pc.gameObject.AddComponent<ExtendedPlayerControl>();
            }
        }
    }

    internal static ExtendedPlayerControl? BetterPlayerControl(this PlayerControl player)
    {
        return MonoExtensionManager.Get<ExtendedPlayerControl>(player);
    }

    internal static ExtendedPlayerControl? BetterPlayerControl(this PlayerPhysics playerPhysics)
    {
        return MonoExtensionManager.Get<ExtendedPlayerControl>(playerPhysics.myPlayer);
    }
}