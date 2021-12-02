using SCKRM.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCKRM.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("커널/UI/Layout/수평 레이아웃")]
    public sealed class HorizontalLayout : MonoBehaviour
    {
        [SerializeField, HideInInspector] RectTransform _rectTransform;
        public RectTransform rectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = GetComponent<RectTransform>();

                return _rectTransform;
            }
        }


        [SerializeField] RectOffset _padding = new RectOffset();
        public RectOffset padding { get => _padding; set => _padding = value; }
        [SerializeField, Min(0)] float _spacing;
        public float spacing { get => _spacing; set => _spacing = value; }


        public RectTransform[] childRectTransforms { get; private set; }
        public HorizontalLayoutSetting[] childHorizontalLayoutSettings { get; private set; }



        [System.NonSerialized] int tempChildCount = -1;
        void Update()
        {
            {
#if UNITY_EDITOR
                if (tempChildCount != transform.childCount || !Application.isPlaying)
                {
                    SetChild();
                    tempChildCount = transform.childCount;
                }
#else
                if (tempChildCount != transform.childCount)
                {
                    SetChild();
                    tempChildCount = transform.childCount;
                }
#endif
                for (int i = 0; i < childRectTransforms.Length; i++)
                {
                    RectTransform rectTransform = childRectTransforms[i];
                    if (i != rectTransform.GetSiblingIndex())
                        SetChild();
                }

                void SetChild()
                {
                    childRectTransforms = new RectTransform[transform.childCount];
                    childHorizontalLayoutSettings = new HorizontalLayoutSetting[childRectTransforms.Length];
                    for (int i = 0; i < childRectTransforms.Length; i++)
                    {
                        Transform childTransform = transform.GetChild(i);
                        childRectTransforms[i] = childTransform.GetComponent<RectTransform>();
                        childHorizontalLayoutSettings[i] = childTransform.GetComponent<HorizontalLayoutSetting>();
                    }
                }
            }

            if (childRectTransforms == null || childRectTransforms.Length <= 0)
                return;

            bool center = false;
            bool right = false;
            float x = 0;
            for (int i = 0; i < childRectTransforms.Length; i++)
            {
                RectTransform childRectTransform = childRectTransforms[i];
                if (childRectTransform == null)
                    continue;
                else if (!childRectTransform.gameObject.activeSelf)
                    continue;
                else if (childRectTransform.sizeDelta.x == 0 || childRectTransform.sizeDelta.y == 0)
                    continue;

                HorizontalLayoutSetting taskBarLayoutSetting = childHorizontalLayoutSettings[i];
                if (taskBarLayoutSetting != null)
                {
                    if (taskBarLayoutSetting.custom)
                        continue;

                    if (!right && taskBarLayoutSetting.mode == HorizontalLayoutSetting.Mode.right)
                    {
                        right = true;
                        x = 0;
                    }
                    else if (!center && taskBarLayoutSetting.mode == HorizontalLayoutSetting.Mode.center)
                    {
                        center = true;

                        x = 0;
                        x += (childRectTransform.sizeDelta.x + (_padding.left - _padding.right) - spacing) * 0.5f;
                        for (int j = i; j < childRectTransforms.Length; j++)
                        {
                            RectTransform rectTransform2 = childRectTransforms[j];
                            if (!rectTransform2.gameObject.activeSelf)
                                continue;
                            if (rectTransform2 == null)
                                continue;
                            else if (!rectTransform2.gameObject.activeSelf)
                                continue;
                            else if (rectTransform2.sizeDelta.x == 0 || rectTransform2.sizeDelta.y == 0)
                                continue;

                            HorizontalLayoutSetting taskBarLayoutSetting2 = childHorizontalLayoutSettings[j];
                            if (taskBarLayoutSetting2 != null && taskBarLayoutSetting2.mode == HorizontalLayoutSetting.Mode.right)
                                break;

                            x -= rectTransform2.sizeDelta.x * 0.5f;
                        }
                    }
                }

                if (right)
                {
                    childRectTransform.anchorMin = new Vector2(1, 0.5f);
                    childRectTransform.anchorMax = new Vector2(1, 0.5f);
                    childRectTransform.pivot = new Vector2(1, 0.5f);
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        childRectTransform.anchoredPosition = new Vector2(x - padding.right, -(padding.top - padding.bottom) * 0.5f);
                    else
                        childRectTransform.anchoredPosition = childRectTransform.anchoredPosition.Lerp(new Vector2(x - padding.right, -(padding.top - padding.bottom) * 0.5f), 0.2f * Kernel.fpsDeltaTime);
#else
                    rectTransform.anchoredPosition = rectTransform.anchoredPosition.Lerp(new Vector2(x, -(padding.top - padding.bottom) * 0.5f), 0.2f * Kernel.fpsDeltaTime);
#endif
                    x -= childRectTransform.sizeDelta.x + spacing;
                }
                else if (center)
                {
                    childRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    childRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    childRectTransform.pivot = new Vector2(0.5f, 0.5f);
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        childRectTransform.anchoredPosition = new Vector2(x, -(padding.top - padding.bottom) * 0.5f);
                    else
                        childRectTransform.anchoredPosition = childRectTransform.anchoredPosition.Lerp(new Vector2(x, -(padding.top - padding.bottom) * 0.5f), 0.2f * Kernel.fpsDeltaTime);
#else
                    rectTransform.anchoredPosition = rectTransform.anchoredPosition.Lerp(new Vector2(x, -(padding.top - padding.bottom) * 0.5f), 0.2f * Kernel.fpsDeltaTime);
#endif
                    x += childRectTransform.sizeDelta.x + spacing;
                }
                else
                {
                    childRectTransform.anchorMin = new Vector2(0, 0.5f);
                    childRectTransform.anchorMax = new Vector2(0, 0.5f);
                    childRectTransform.pivot = new Vector2(0, 0.5f);
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        childRectTransform.anchoredPosition = new Vector2(x + padding.left, -(padding.top - padding.bottom) * 0.5f);
                    else
                        childRectTransform.anchoredPosition = childRectTransform.anchoredPosition.Lerp(new Vector2(x + padding.left, -(padding.top - padding.bottom) * 0.5f), 0.2f * Kernel.fpsDeltaTime);
#else
                    rectTransform.anchoredPosition = rectTransform.anchoredPosition.Lerp(new Vector2(x + padding.left, -(padding.top - padding.bottom) * 0.5f), 0.2f * Kernel.fpsDeltaTime);
#endif
                    x += childRectTransform.sizeDelta.x + spacing;
                }

                childRectTransform.sizeDelta = new Vector3(childRectTransform.sizeDelta.x, rectTransform.sizeDelta.y - padding.top - padding.bottom);
            }
        }
    }
}