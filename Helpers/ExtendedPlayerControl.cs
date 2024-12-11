using HarmonyLib;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Helpers;

public class BetterPlayerControl(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    public PlayerControl? _Player { get; set; }
    public TextMeshPro? InfoTextInfo { get; set; }
    public TextMeshPro? InfoTextTop { get; set; }
    public TextMeshPro? InfoTextBottom { get; set; }
}

public static class PlayerControlExtension
{
    [HarmonyPatch(typeof(PlayerControl))]
    class PlayerControlPatch
    {
        [HarmonyPatch(nameof(PlayerControl.Awake))]
        [HarmonyPrefix]
        public static void Awake_Prefix(PlayerControl __instance)
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

        public static void TryCreateExtendedPlayerControl(PlayerControl pc, TextMeshPro InfoText_Info_TMP, TextMeshPro InfoText_T_TMP, TextMeshPro InfoText_B_TMP)
        {
            if (pc.BetterPlayerControl() == null)
            {
                BetterPlayerControl newExtendedPc = pc.gameObject.AddComponent<BetterPlayerControl>();
                newExtendedPc._Player = pc;
                newExtendedPc.InfoTextInfo = InfoText_Info_TMP;
                newExtendedPc.InfoTextTop = InfoText_T_TMP;
                newExtendedPc.InfoTextBottom = InfoText_B_TMP;
            }
        }
    }

    public static BetterPlayerControl? BetterPlayerControl(this PlayerControl player)
    {
        return player?.GetComponent<BetterPlayerControl>();
    }

    public static BetterPlayerControl? BetterPlayerControl(this PlayerPhysics playerPhysics)
    {
        return playerPhysics?.GetComponent<BetterPlayerControl>();
    }
}