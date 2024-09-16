using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches;

class SplashIntroPatch
{
    public static bool BetterIntro = false;
    public static bool IsReallyDoneLoading = false;
    private static float ShowTime = 0f;
    private static GameObject BetterLogo;

    [HarmonyPatch(typeof(SplashManager))]
    class SplashManagerPatch
    {
        [HarmonyPatch(nameof(SplashManager.Start))]
        [HarmonyPrefix]
        public static void Start_Prefix(SplashManager __instance)
        {
            __instance.logoAnimFinish.transform.Find("BlackOverlay").transform.SetLocalY(100f);
        }

        [HarmonyPatch(nameof(SplashManager.Update))]
        [HarmonyPrefix]
        public static bool Update_Prefix(SplashManager __instance)
        {
            ShowTime += Time.deltaTime;

            if (ShowTime > 6f && BetterIntro)
            {
                UnityEngine.Object.Destroy(__instance.logoAnimFinish.GetComponent<AudioSource>());
            }

            if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (ShowTime > 2.5f && !IsReallyDoneLoading)
                {
                    IsReallyDoneLoading = true;
                    __instance.startedSceneLoad = true;
                    __instance.sceneChanger.BeginLoadingScene();
                    __instance.sceneChanger.AllowFinishLoadingScene();
                    __instance.logoAnimFinish.gameObject.SetActive(false);
                    __instance.loadingObject.SetActive(true);
                }
            }

            if (__instance.doneLoadingRefdata && !__instance.startedSceneLoad && Time.time - __instance.startTime > __instance.minimumSecondsBeforeSceneChange)
            {
                if (!BetterIntro)
                {
                    __instance.startTime = Time.time;
                    __instance.logoAnimFinish.gameObject.SetActive(false);
                    __instance.logoAnimFinish.gameObject.SetActive(true);
                    GameObject InnerLogo = __instance.logoAnimFinish.transform.Find("LogoRoot/ISLogo").gameObject;
                    BetterLogo = UnityEngine.Object.Instantiate(InnerLogo, InnerLogo.transform.parent);
                    GameObject.Destroy(InnerLogo);
                    BetterLogo.name = "BetterLogo";
                    BetterLogo.GetComponent<SpriteRenderer>().sprite = Utils.LoadSprite("BetterAmongUs.Resources.Images.BetterAmongUs-By-The-Enhanced-Network-Logo.png", 150f);
                    __instance.logoAnimFinish.transform.Find("BlackOverlay").transform.SetLocalY(0f);
                    __instance.loadingObject.SetActive(false);
                }

                __instance.startedSceneLoad = BetterIntro;
                BetterIntro = true;

                if (__instance.startedSceneLoad && BetterIntro)
                {
                    __instance.sceneChanger.AllowFinishLoadingScene();
                    __instance.startedSceneLoad = true;
                    __instance.loadingObject.SetActive(true);
                    IsReallyDoneLoading = true;
                }
            }

            return false;
        }
    }
}