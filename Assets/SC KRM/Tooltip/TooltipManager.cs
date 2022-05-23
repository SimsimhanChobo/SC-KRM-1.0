using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using SCKRM.Input;
using SCKRM.Resource;
using SCKRM.SaveLoad;
using SCKRM.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SCKRM.Tooltip
{
    public class TooltipManager : ManagerUI<TooltipManager>
    {
        public static bool isShow { get; private set; } = false;



        [SerializeField, NotNull] RectTransform toolTip;
        [SerializeField, NotNull] SetSizeAsTargetRectTransform toolTipSetSizeAsTargetRectTransform;
        [SerializeField, NotNull] CanvasGroup toolTipCanvasGroup;
        [SerializeField, NotNull] TMP_Text toolTipText;
        [SerializeField, NotNull] BetterContentSizeFitter toolTipTextBetterContentSizeFitter;

        DrivenRectTransformTracker tracker;



        protected override void Awake() => SingletonCheck(this);

        void LateUpdate()
        {
            tracker.Clear();
            tracker.Add(this, toolTip, DrivenTransformProperties.AnchoredPosition3D);

            if (isShow)
                toolTipCanvasGroup.alpha += 0.1f * Kernel.fpsDeltaTime;
            else
                toolTipCanvasGroup.alpha -= 0.1f * Kernel.fpsDeltaTime;

            Vector2 cursorScale = CursorManager.instance.rectTransform.localScale;
            Vector2 cursorSize = CursorManager.instance.rectTransform.rect.size;

            toolTipCanvasGroup.alpha = toolTipCanvasGroup.alpha.Clamp01();
            toolTip.anchoredPosition = InputManager.mousePosition + (new Vector2(cursorSize.x, -cursorSize.y) * cursorScale);
        }

        protected override void OnDisable() => tracker.Clear();

        public static void Show(string text, string nameSpace = "")
        {
            isShow = true;
            instance.toolTipText.text = ResourceManager.SearchLanguage(text, nameSpace);
            if (instance.toolTipText.text == "")
                instance.toolTipText.text = text;
            if (instance.toolTipText.text == "")
                return;

            if (instance.toolTipCanvasGroup.alpha <= 0)
            {
                instance.toolTipTextBetterContentSizeFitter.SetLayoutHorizontal();
                instance.toolTipTextBetterContentSizeFitter.SetLayoutVertical();

                instance.toolTipSetSizeAsTargetRectTransform.LayoutRefresh();
                instance.toolTipSetSizeAsTargetRectTransform.SizeUpdate(false);
            }
        }

        public static void Hide() => isShow = false;
    }
}
