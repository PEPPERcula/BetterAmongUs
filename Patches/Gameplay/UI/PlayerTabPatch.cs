using BetterAmongUs.Data;
using BetterAmongUs.Data.Json;
using BetterAmongUs.Helpers;
using BetterAmongUs.Patches.Client;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Patches.Gameplay.UI;

[HarmonyPatch(typeof(PlayerTab))]
internal static class PlayerTabPatch
{
    private static List<PassiveButton> presetButtons = [];
    private static float cooldown = 0f;

    [HarmonyPatch(nameof(PlayerTab.OnEnable))]
    [HarmonyPrefix]
    private static void OnEnable_Postfix(PlayerTab __instance)
    {
        foreach (var button in presetButtons.ToArray())
        {
            if (button == null) continue;
            UnityEngine.Object.Destroy(button.gameObject);
        }
        presetButtons.Clear();

        for (int i = 0; i <= 5; i++)
        {
            int currentI = i;
            var name = currentI == 0 ? "Among Us Preset" : $"Preset {i}";
            var button = __instance.CreateButton(name, new Vector3(2.5f, 1.55f - currentI * 0.45f, 0f), () =>
            {
                if (cooldown > 0f || BetterDataManager.BetterDataFile.SelectedOutfitPreset == currentI) return;
                cooldown = 0.5f;

                BetterDataManager.BetterDataFile.SelectedOutfitPreset = currentI;

                foreach (var button in presetButtons)
                {
                    if (button == null) continue;
                    button.SetPassiveButtonHoverStateInactive();
                }

                var data = OutfitData.GetOutfitData(currentI);
                data.Load(() =>
                {
                    if (LoadPlayerOutfit(data))
                    {
                        __instance.PlayerPreview.UpdateFromLocalPlayer(PlayerMaterial.MaskType.None);
                    }
                    else
                    {
                        __instance.PlayerPreview.UpdateFromDataManager(PlayerMaterial.MaskType.None);
                    }
                });
            });
            presetButtons.Add(button);
        }
    }

    private static bool LoadPlayerOutfit(OutfitData data)
    {
        var player = PlayerControl.LocalPlayer;
        if (player != null)
        {
            player.RpcSetHat(data.HatId);
            player.RpcSetPet(data.PetId);
            player.RpcSetSkin(data.SkinId);
            player.RpcSetVisor(data.VisorId);
            player.RpcSetNamePlate(data.NamePlateId);
            return true;
        }

        return false;
    }

    private static PassiveButton CreateButton(this PlayerTab __instance, string name, Vector3 pos, Action callback)
    {
        var button = UnityEngine.Object.Instantiate(MainMenuPatch.ButtonPrefab, __instance.transform);
        button.gameObject.SetActive(true);
        button.gameObject.SetLayers("UI");
        button.transform.localPosition = pos;
        button.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        button.OnClick = new();
        button.OnClick.AddListener(callback);
        button.DestroyTextTranslators();
        var text = button.GetComponentInChildren<TextMeshPro>();
        text?.SetText(name);
        return button;
    }

    [HarmonyPatch(nameof(PlayerTab.Update))]
    [HarmonyPrefix]
    private static void Updatee_Postfix(PlayerTab __instance)
    {
        if (cooldown > 0f)
        {
            cooldown -= Time.deltaTime;
        }
        else
        {
            cooldown = 0f;
        }

        for (int i = 0; i < presetButtons.Count; i++)
        {
            PassiveButton? button = presetButtons[i];
            if (button == null) continue;
            if (i == BetterDataManager.BetterDataFile.SelectedOutfitPreset)
            {
                button.SetPassiveButtonHoverStateActive();
            }
        }
    }
}
