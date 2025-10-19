using BetterAmongUs.Helpers;
using BetterAmongUs.Items;
using BetterAmongUs.Modules;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BetterAmongUs.Patches.Client;

[HarmonyPatch(typeof(MainMenuManager))]
internal class MainMenuPatch
{
    private static List<PassiveButton> buttons = [];
    private static List<GameObject> buttonsObj = [];
    private static PassiveButton? template;
    private static PassiveButton? creditsButton;
    private static PassiveButton? gitHubButton;
    private static PassiveButton? discordButton;

    private static SpriteRenderer? sprite;
    [HarmonyPatch(nameof(MainMenuManager.LateUpdate))]
    [HarmonyPostfix]
    internal static void LateUpdate_Postfix(MainMenuManager __instance)
    {
        if (BannedUserData.IsBanned || FileChecker.HasUnauthorizedFileOrMod)
        {
            __instance.playButton.enabled = false;
            sprite ??= __instance.playButton.transform.Find("Inactive").GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                sprite.color = new Color(0.7f, 0.7f, 0.7f);
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
            }

            SceneManager.s_AllowLoadScene = true;
        }

        bool Flag = __instance?.screenTint?.GetComponent<SpriteRenderer>() != null
            && !__instance.screenTint.GetComponent<SpriteRenderer>().enabled;

        foreach (var item in buttonsObj)
        {
            item.SetActive(Flag);
        }
    }

    // Replace AU logo with BAU logo
    [HarmonyPatch(nameof(MainMenuManager.Start))]
    [HarmonyPostfix]
    internal static void Postfix(MainMenuManager __instance)
    {
        GameObject logo = GameObject.Find("LeftPanel/Sizer/LOGO-AU");
        GameObject sizer = logo.transform.parent.gameObject;
        sizer.transform.localPosition = new Vector3(sizer.transform.localPosition.x, sizer.transform.localPosition.y - 0.035f, sizer.transform.localPosition.z);
        sizer.transform.position = new Vector3(sizer.transform.position.x, sizer.transform.position.y, -0.5f);
        logo.transform.localScale = new Vector3(0.003f, 0.0025f, 0f);
        logo.GetComponent<SpriteRenderer>().sprite = Utils.LoadSprite("BetterAmongUs.Resources.Images.BetterAmongUs-Logo.png", 1f);

        if (template == null) template = __instance.quitButton;

        if (template == null) return;

        buttons.Clear();
        buttonsObj.Clear();

        /*
        if (creditsButton == null)
        {
            creditsButton = CreateButton(
                "CreditsButton",
                new(255, 255, 255, byte.MaxValue),
                new(200, 200, 200, byte.MaxValue),
                () =>
                {
                    DisconnectPopup.Instance.ShowCustom(creditsTextTitle + creditsText);
                },
                "Mod Credits"); //"Credits"
        }
        */

        if (gitHubButton == null)
        {
            gitHubButton = CreateButton(
                "GitHubButton",
                new(153, 153, 153, byte.MaxValue),
                new(209, 209, 209, byte.MaxValue),
                () => Application.OpenURL(ModInfo.Github),
                "GitHub"); //"GitHub"
        }

        if (discordButton == null)
        {
            discordButton = CreateButton(
                "DiscordButton",
                new(88, 101, 242, byte.MaxValue),
                new(148, 161, byte.MaxValue, byte.MaxValue),
                () => Application.OpenURL(ModInfo.Discord),
                "Discord"); //"Discord"
        }
    }

    internal static PassiveButton CreateButton(string name, Color32 normalColor, Color32 hoverColor, Action action, string label, Vector2? scale = null)
    {
        var button = UnityEngine.Object.Instantiate(template);
        buttons.Add(button);
        buttonsObj.Add(button.gameObject);
        button.name = name;
        button.GetComponent<AspectPosition>().DestroyMono();

        // Set button position and scale
        float baseY = -2.7882f;
        float newY = baseY + 0.38f * (buttons.Count - 1);
        button.transform.localPosition = new Vector3(-0.6118f, newY, -5f);
        button.transform.localScale = new Vector3(0.78f, 0.78f, 0.78f);

        // Set button action
        button.OnClick = new();
        button.OnClick.AddListener(action);

        // Set button text
        var buttonText = button.transform.Find("FontPlacer/Text_TMP").GetComponent<TMP_Text>();
        buttonText.DestroyTextTranslators();
        buttonText.fontSize = buttonText.fontSizeMax = buttonText.fontSizeMin = 3.5f;
        buttonText.enableWordWrapping = false;
        buttonText.text = label;

        // Set button colors
        var normalSprite = button.inactiveSprites.GetComponent<SpriteRenderer>();
        var hoverSprite = button.activeSprites.GetComponent<SpriteRenderer>();
        normalSprite.color = normalColor;
        hoverSprite.color = hoverColor;

        // Align text
        var container = buttonText.transform.parent;
        container.GetComponent<AspectPosition>().DestroyMono();
        buttonText.GetComponent<AspectPosition>().DestroyMono();
        container.SetLocalX(0f);
        buttonText.transform.SetLocalX(0f);
        buttonText.horizontalAlignment = HorizontalAlignmentOptions.Center;

        // Set button collider
        var buttonCollider = button.GetComponent<BoxCollider2D>();
        if (scale.HasValue)
        {
            normalSprite.size = hoverSprite.size = buttonCollider.size = scale.Value;
        }

        buttonCollider.offset = new Vector2(0f, 0f);

        return button;
    }
}
