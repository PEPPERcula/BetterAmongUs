using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using BetterAmongUs.Network.Configs;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BetterAmongUs.Patches.Client;

[HarmonyPatch(typeof(MainMenuManager))]
internal static class MainMenuPatch
{
    private static SpriteRenderer? sprite;
    internal static PassiveButton? ButtonPrefab;

    [HarmonyPatch(nameof(MainMenuManager.LateUpdate))]
    [HarmonyPostfix]
    private static void LateUpdate_Postfix(MainMenuManager __instance)
    {
        if (BannedUserData.IsBanned || FileChecker.HasUnauthorizedFileOrMod)
        {
            __instance.playButton.enabled = false;
            sprite ??= __instance.playButton.transform.Find("Inactive").GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                sprite.color = new Color(0.7f, 0.7f, 0.7f);
                ObjectHelper.AddColor(sprite);
            }

            SceneManager.s_AllowLoadScene = false;
        }
        else
        {
            __instance.playButton.enabled = true;
            sprite ??= __instance.playButton.transform.Find("Inactive").GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                sprite.color = Color.white;
                ObjectHelper.AddColor(sprite);
            }

            SceneManager.s_AllowLoadScene = true;
        }


        List<PassiveButton> buttons = [__instance.playButton, __instance.inventoryButton, __instance.shopButton, __instance.playLocalButton, __instance.PlayOnlineButton, __instance.backButtonOnline,
            __instance.newsButton, __instance.myAccountButton, __instance.settingsButton, __instance.howToPlayButton, __instance.freePlayButton];
        foreach (var button in buttons)
        {
            button.gameObject?.SetUIColors(sprite =>
            {
                return sprite.color == Color.white;
            },
            "Icon", "Background");
        }

        /*
        bool Flag = __instance?.screenTint?.GetComponent<SpriteRenderer>() != null
            && !__instance.screenTint.GetComponent<SpriteRenderer>().enabled;
        */
    }

    // Replace AU logo with BAU logo
    [HarmonyPatch(nameof(MainMenuManager.Start))]
    [HarmonyPostfix]
    private static void Start_Postfix(MainMenuManager __instance)
    {
        GameObject logo = GameObject.Find("LeftPanel/Sizer/LOGO-AU");
        GameObject sizer = logo.transform.parent.gameObject;
        sizer.transform.localPosition = new Vector3(sizer.transform.localPosition.x, sizer.transform.localPosition.y - 0.035f, sizer.transform.localPosition.z);
        sizer.transform.position = new Vector3(sizer.transform.position.x, sizer.transform.position.y, -0.5f);
        logo.transform.localScale = new Vector3(0.003f, 0.0025f, 0f);
        logo.GetComponent<SpriteRenderer>().sprite = Utils.LoadSprite("BetterAmongUs.Resources.Images.BetterAmongUs-Logo.png", 1f);

        __instance.transform.Find("MainUI/AspectScaler/BackgroundTexture")?.gameObject?.SetSpriteColors(sprite => ObjectHelper.AddColor(sprite));

        if (ButtonPrefab == null)
        {
            ButtonPrefab = UnityEngine.Object.Instantiate(__instance.inventoryButton);
            ButtonPrefab.gameObject.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(ButtonPrefab);
        }
    }
}
