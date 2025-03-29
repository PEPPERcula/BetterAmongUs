using HarmonyLib;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Helpers;

internal class ExtendedPlayerControl : MonoBehaviour
{
    internal static readonly HashSet<(PlayerControl player, ExtendedPlayerControl extendedPlayer)> AllExtendedPlayerControl = [];

    internal PlayerControl? _Player { get; set; }
    internal TextMeshPro? InfoTextInfo { get; set; }
    internal TextMeshPro? InfoTextTop { get; set; }
    internal TextMeshPro? InfoTextBottom { get; set; }

    internal void Start()
    {
        var player = gameObject.GetComponent<PlayerControl>();
        if (player != null)
        {
            if (!AllExtendedPlayerControl.Any(items => items.extendedPlayer == this))
            {
                AllExtendedPlayerControl.Add((player, this));
            }
        }
        else
        {
            Destroy(this);
        }
    }

    internal void OnDestroy()
    {
        AllExtendedPlayerControl.RemoveWhere(items => items.extendedPlayer == this);
    }
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
            var nameTextTransform = __instance.gameObject.transform.Find("Names/NameText_TMP");
            var nameText = nameTextTransform?.GetComponent<TextMeshPro>();

            TextMeshPro InstantiatePlayerInfoText(string name, Vector3 positionOffset)
            {
                var newTextObject = UnityEngine.Object.Instantiate(nameText, nameTextTransform);
                newTextObject.name = name;
                newTextObject.transform.DestroyChildren();
                newTextObject.transform.position += positionOffset;
                var textMesh = newTextObject.GetComponent<TextMeshPro>();
                if (textMesh != null)
                {
                    textMesh.text = string.Empty;
                }
                newTextObject.gameObject.SetActive(true);
                return newTextObject;
            }

            var text1 = InstantiatePlayerInfoText("InfoText_Info_TMP", new Vector3(0f, 0.25f));
            var text2 = InstantiatePlayerInfoText("InfoText_T_TMP", new Vector3(0f, 0.15f));
            var text3 = InstantiatePlayerInfoText("InfoText_B_TMP", new Vector3(0f, -0.15f));

            TryCreateExtendedPlayerControl(__instance, text1, text2, text3);
        }

        internal static void TryCreateExtendedPlayerControl(PlayerControl pc, TextMeshPro InfoText_Info_TMP, TextMeshPro InfoText_T_TMP, TextMeshPro InfoText_B_TMP)
        {
            if (pc.BetterPlayerControl() == null)
            {
                ExtendedPlayerControl newExtendedPc = pc.gameObject.AddComponent<ExtendedPlayerControl>();
                newExtendedPc._Player = pc;
                newExtendedPc.InfoTextInfo = InfoText_Info_TMP;
                newExtendedPc.InfoTextTop = InfoText_T_TMP;
                newExtendedPc.InfoTextBottom = InfoText_B_TMP;
            }
        }
    }

    internal static ExtendedPlayerControl? BetterPlayerControl(this PlayerControl player)
    {
        return ExtendedPlayerControl.AllExtendedPlayerControl.FirstOrDefault(items => items.player == player).extendedPlayer ?? null;
    }

    internal static ExtendedPlayerControl? BetterPlayerControl(this PlayerPhysics playerPhysics)
    {
        return ExtendedPlayerControl.AllExtendedPlayerControl.FirstOrDefault(items => items.player == playerPhysics.myPlayer).extendedPlayer ?? null;
    }
}