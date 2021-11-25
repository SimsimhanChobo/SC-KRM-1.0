using Cysharp.Threading.Tasks;
using SCKRM.Resource;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SCKRM.Splash
{
    public sealed class SplashScreen : MonoBehaviour
    {
        public static bool isAniPlayed { get; private set; } = true;
        [SerializeField] Transform Logo;
        [SerializeField] Image LogoImage;
        [SerializeField] Transform CS;
        [SerializeField] Image CSImage;
        [SerializeField] Text text;

        float xV;
        float yV;
        float rV;

        bool xFlip = false;
        bool aniEnd = false;

        float timer = 0;

        bool aniPlay = false;

        AudioClip bow;
        AudioClip drawmap;

        async void Awake()
        {
            bow = await ResourceManager.GetAudio(KernelMethod.PathCombine(Kernel.streamingAssetsPath, ResourceManager.soundPath.Replace("%NameSpace%", "minecraft"), "random/bow"));
            drawmap = await ResourceManager.GetAudio(KernelMethod.PathCombine(Kernel.streamingAssetsPath, ResourceManager.soundPath.Replace("%NameSpace%", "minecraft"), "ui/cartography_table/drawmap") + Random.Range(1, 4));

            isAniPlayed = true;
            aniPlay = false;

            if (Random.Range(0, 2) == 1)
                xFlip = true;
            else
                xFlip = false;

            if (xFlip)
            {
                CS.localPosition = new Vector2(670, Random.Range(-32f, 245f));
                xV = Random.Range(-8f, -20f);
                rV = Random.Range(10f, 20f);
            }
            else
            {
                CS.localPosition = new Vector2(-670, Random.Range(-32f, 245f));
                xV = Random.Range(8f, 20f);
                rV = Random.Range(-10f, -20f);
            }

            yV = Random.Range(8f, 15f);

            timer = 0;
            aniEnd = false;

            aniPlay = true;
            await UniTask.WaitUntil(() => alpha >= 1);

            AudioSource.PlayClipAtPoint(bow, Vector3.zero);
        }

        float alpha = 0;
        void Update()
        {
            if (!aniPlay)
                return;

            if (alpha < 1 && !aniEnd)
            {
                LogoImage.color = new Color(1, 1, 1, alpha);
                CSImage.color = new Color(1, 1, 1, alpha);
                text.color = new Color(1, 1, 1, alpha);
                alpha += 0.05f * Kernel.fpsDeltaTime;

                return;
            }

            if (aniEnd)
            {
                text.rectTransform.anchoredPosition = text.rectTransform.anchoredPosition.Lerp(Vector3.zero, 0.1f * Kernel.fpsDeltaTime);
                CSImage.transform.rotation = Quaternion.Lerp(CSImage.transform.rotation, Quaternion.Euler(Vector3.zero), 0.1f * Kernel.fpsDeltaTime);
                CS.localPosition = CS.localPosition.Lerp(new Vector3(24, -24), 0.1f * Kernel.fpsDeltaTime);

                if (timer >= 2)
                {
                    LogoImage.color = new Color(1, 1, 1, alpha);
                    CSImage.color = new Color(1, 1, 1, alpha);
                    text.color = new Color(1, 1, 1, alpha);
                    alpha -= 0.05f * Kernel.fpsDeltaTime;

                    if (alpha < 0)
                        isAniPlayed = false;
                }
                else
                    timer += Kernel.deltaTime;
            }
            else
            {
                LogoImage.color = Color.white;
                CSImage.color = Color.white;
                text.color = Color.white;
                alpha = 1;

                CS.localPosition = new Vector2(CS.localPosition.x + xV * Kernel.fpsDeltaTime, CS.localPosition.y + yV * Kernel.fpsDeltaTime);
                CSImage.transform.localEulerAngles = new Vector3(CSImage.transform.localEulerAngles.x, CSImage.transform.localEulerAngles.y, CSImage.transform.localEulerAngles.z + rV * Kernel.fpsDeltaTime);
                yV -= 0.5f * Kernel.fpsDeltaTime;

                if (CS.localPosition.x >= -25 && CS.localPosition.x <= 25 && CS.localPosition.y >= -25 && CS.localPosition.y <= 25)
                {
                    text.rectTransform.anchoredPosition = new Vector3(0, -13);
                    text.text = "TEAM Project";

                    AudioSource.PlayClipAtPoint(drawmap, Vector3.zero);

                    aniEnd = true;
                }
                else if (xFlip && (CS.localPosition.x <= -500 || CS.localPosition.y <= -300))
                {
                    text.rectTransform.anchoredPosition = new Vector3(0, -13);
                    text.text = "TEAM Project";

                    AudioSource.PlayClipAtPoint(drawmap, Vector3.zero);

                    aniEnd = true;
                }
                else if (!xFlip && (CS.localPosition.x >= 500 || CS.localPosition.y <= -300))
                {
                    text.rectTransform.anchoredPosition = new Vector3(0, -13);
                    text.text = "TEAM Project";

                    AudioSource.PlayClipAtPoint(drawmap, Vector3.zero);

                    aniEnd = true;
                }
            }
        }
    }
}