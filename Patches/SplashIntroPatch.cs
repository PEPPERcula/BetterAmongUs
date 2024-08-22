using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;

namespace BetterAmongUs.Patches;

class SplashIntroPatch
{
    public static bool BetterIntro = false;
    public static bool IsReallyDoneLoading = false;
    private static float ShowTime = 0f;

    [HarmonyPatch(typeof(SplashManager))]
    class SplashManagerPatch
    {
        [HarmonyPatch(nameof(SplashManager.Update))]
        [HarmonyPrefix]
        public static bool Update_Prefix(SplashManager __instance)
        {
            ShowTime += Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (ShowTime > 1.5f)
                {
                    __instance.StopAllCoroutines();
                    UnityEngine.Object.Destroy(__instance.logoAnimFinish.gameObject);
                    __instance.sceneChanger.AllowFinishLoadingScene();
                    __instance.startedSceneLoad = true;
                    __instance.loadingObject.SetActive(true);
                    IsReallyDoneLoading = true;
                }
            }

            if (__instance.doneLoadingRefdata && !__instance.startedSceneLoad && Time.time - __instance.startTime > __instance.minimumSecondsBeforeSceneChange)
            {
                if (!BetterIntro)
                {
                    __instance.startTime = Time.time;
                    __instance.logoAnimFinish.gameObject.SetActive(false);
                    __instance.logoAnimFinish.gameObject.SetActive(true);
                    GameObject Innerlogo = __instance.logoAnimFinish.transform.Find("LogoRoot/ISLogo").gameObject;
                    if (Innerlogo != null)
                    {
                        Innerlogo.GetComponent<SpriteRenderer>().sprite = Utils.LoadSprite("BetterAmongUs.Resources.Images.BetterAmongUs-Logo.png", 150f);
                    }
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