using BepInEx.Unity.IL2CPP.Utils;
using BetterAmongUs.Helpers;
using BetterAmongUs.Network.Loaders;
using System.Collections;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Network;

internal class UpdateManager : MonoBehaviour
{
    private bool AmUpdateing;

    internal static UpdateManager? Instance { get; private set; }

    internal static void Init()
    {
        var obj = new GameObject("UpdateManager(BAU)") { hideFlags = HideFlags.HideAndDontSave };
        DontDestroyOnLoad(obj);
        Instance = obj.AddComponent<UpdateManager>();
    }

    internal void OnMainMenu()
    {
        var doNotPress = FindObjectOfType<DoNotPressButton>(true);
        if (doNotPress != null)
        {
            doNotPress.gameObject.SetActive(UpdateLoader.UpdateInfo?.IsNewUpdate() == true);
            doNotPress.pressedSprite = doNotPress.transform.Find("ButtonPressed")?.gameObject?.GetComponent<SpriteRenderer>();
            doNotPress.unpressedSprite = doNotPress.transform.Find("ButtonUnpressed")?.gameObject?.GetComponent<SpriteRenderer>();
            doNotPress.pressedSprite.enabled = false;
            doNotPress.pressedSprite.color = new(0.15f, 0.8f, 0.4f);
            doNotPress.unpressedSprite.color = new(0.15f, 0.8f, 0.4f);
            var button = doNotPress.GetComponent<PassiveButton>();
            if (button != null)
            {
                button.OnClick = new();
                button.OnClick.AddListener((Action)(() =>
                {
                    if (AmUpdateing) return;
                    this.StartCoroutine(CoPressDownload(doNotPress));
                }));
            }

            var obj = new GameObject("Update(TMP)");
            obj.transform.SetParent(doNotPress.transform, false);
            obj.transform.localPosition = new Vector3(-0.1018f, -0.1883f, 0f);
            var text = obj.AddComponent<TextMeshPro>();
            text.color = Color.black;
            text.fontSize = 1.5f;
            text.alignment = TextAlignmentOptions.Center;
            text.horizontalAlignment = HorizontalAlignmentOptions.Center;
            text.SetText("Update");
        }
    }

    private void Start()
    {
        var oldDll = Assembly.GetExecutingAssembly().Location + ".old";
        if (File.Exists(oldDll))
        {
            File.Delete(oldDll);
        }
    }

    private GameObject? mainMenu;
    private GameObject? ambience;

    private IEnumerator CoPressDownload(DoNotPressButton button)
    {
        AmUpdateing = true;

        button.pressedSprite.enabled = true;
        button.unpressedSprite.enabled = false;
        yield return new WaitForSeconds(0.1f);
        button.unpressedSprite.enabled = true;
        button.pressedSprite.enabled = false;
        yield return new WaitForSeconds(0.1f);
        button.gameObject.SetActive(false);

        mainMenu = GameObject.Find("MainMenuManager");
        ambience = GameObject.Find("Ambience");
        mainMenu?.SetActive(false);
        ambience?.SetActive(false);

        if (UpdateLoader.UpdateInfo != null && UpdateLoader.UpdateInfo.DllLink != string.Empty)
        {
            if (UpdateLoader.UpdateInfo.IsNewUpdate())
            {
                yield return UpdateLoader.UpdateInfo.CoDownload();
                mainMenu?.SetActive(true);
                ambience?.SetActive(true);
                yield return new WaitForSeconds(0.2f);
                Utils.ShowPopUp("Update complete\nRestart required!");
            }
        }
        else
        {
            mainMenu?.SetActive(true);
            ambience?.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            Utils.ShowPopUp("Download link missing!");
        }
    }
}
