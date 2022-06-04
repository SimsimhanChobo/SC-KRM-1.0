using SCKRM.Input;
using SCKRM.Object;
using SCKRM.Renderer;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SCKRM.UI
{
    [ExecuteAlways]
    [AddComponentMenu("커널/UI/드롭다운")]
    public class Dropdown : UI, IPointerEnterHandler, IPointerExitHandler
    {
        bool _isShow = false;
        public bool isShow
        {
            get => _isShow;
            set
            {
                if (value)
                    Toggle();
                else
                    Hide();
            }
        }

        [SerializeField] string[] _options = new string[0];
        public string[] options { get => _options; set => _options = value; }

        [SerializeField] string[] _customLabel = new string[0];
        public string[] customLabel { get => _customLabel; set => _customLabel = value; }

        [SerializeField, Min(0)] int _value = 0;
        public int value { get => _value; set => _value = value; }

        [SerializeField] UnityEvent _onValueChanged = new UnityEvent();
        public UnityEvent onValueChanged { get => _onValueChanged; }

        [SerializeField, NotNull] TMP_Text label;
        [SerializeField, NotNull] RectTransform listRectTransform;
        [SerializeField, NotNull] SetSizeAsTargetRectTransform listSetSizeAsTargetRectTransform;
        [SerializeField, NotNull] DropdownItem template;
        [SerializeField, NotNull] RectTransform viewport;
        [SerializeField, NotNull] Transform content;
        [SerializeField, NotNull] RectTransform scrollbar;
        [SerializeField, NotNull] GameObject scrollbarHandle;

        bool pointer;
        bool mouseDrag = false;
        Vector2 tempMousePos;
        bool invokeLock = false;

        List<DropdownItem> dropdownItems = new List<DropdownItem>();



        DrivenRectTransformTracker tracker;

        void Update()
        {
            if (!Kernel.isPlaying)
            {
                tracker.Clear();
                tracker.Add(this, listRectTransform, DrivenTransformProperties.SizeDeltaX);
                tracker.Add(this, viewport, DrivenTransformProperties.SizeDeltaX);
            }

            if (options.Length > 0)
            {
                value = value.Clamp(0, options.Length - 1);

                if (customLabel.Length > value)
                {
                    label.text = customLabel[value];
                    if (string.IsNullOrEmpty(label.text))
                        label.text = options[value];
                }
                else
                    label.text = options[value];
            }
            else
            {
                value = 0;
                label.text = "";
            }

            if (options.Length >= 9)
            {
                if (!scrollbarHandle.activeSelf)
                    scrollbarHandle.SetActive(true);

                viewport.offsetMax = new Vector2(-scrollbar.sizeDelta.x, 0);
            }
            else
            {
                if (scrollbarHandle.activeSelf)
                    scrollbarHandle.SetActive(false);

                viewport.offsetMax = Vector2.zero;
            }

            if (Kernel.isPlaying)
            {
                if (UnityEngine.Input.GetMouseButtonDown(0))
                    tempMousePos = InputManager.mousePosition;
                else if (!mouseDrag)
                    mouseDrag = UnityEngine.Input.GetMouseButton(0) && Vector2.Distance(InputManager.mousePosition, tempMousePos) >= 10;

                if (!isShow)
                {
                    listRectTransform.sizeDelta = listRectTransform.sizeDelta.Lerp(new Vector2(listRectTransform.sizeDelta.x, listRectTransform.anchoredPosition.y), listSetSizeAsTargetRectTransform.lerpValue * Kernel.fpsUnscaledDeltaTime);

                    if (listRectTransform.gameObject.activeSelf && listRectTransform.sizeDelta.y < listRectTransform.anchoredPosition.y + 0.01f)
                    {
                        for (int i = 0; i < dropdownItems.Count; i++)
                            dropdownItems[i].Remove();

                        listRectTransform.gameObject.SetActive(false);
                    }
                }
                else if (!pointer && !mouseDrag && UnityEngine.Input.GetMouseButtonUp(0))
                    Hide();
                else if (UnityEngine.Input.GetMouseButtonUp(0))
                    mouseDrag = false;

                if (isShow && !listRectTransform.gameObject.activeSelf)
                    listRectTransform.gameObject.SetActive(true);
            }
        }

        protected override void OnEnable() => Hide();

        protected override void OnDisable()
        {
            if (!Kernel.isPlaying)
                tracker.Clear();

            Hide();
        }

        public void Toggle()
        {
            if (isShow)
            {
                Hide();
                return;
            }

            UIManager.BackEventAdd(Hide, true);
            UIManager.homeEvent += Hide;

            listSetSizeAsTargetRectTransform.enabled = true;

            invokeLock = true;

            for (int i = 0; i < dropdownItems.Count; i++)
                dropdownItems[i].Remove();

            dropdownItems.Clear();

            string templateName = "dropdown_" + GetInstanceID() + "_template";
            for (int i = 0; i < options.Length; i++)
            {
                string option = options[i];

                DropdownItem dropdownItem;
                if (ObjectPoolingSystem.ObjectContains(templateName))
                    dropdownItem = (DropdownItem)ObjectPoolingSystem.ObjectCreate(templateName, content).monoBehaviour;
                else
                {
                    ObjectPoolingSystem.ObjectAdd(templateName, template, template);
                    dropdownItem = (DropdownItem)ObjectPoolingSystem.ObjectCreate(templateName, content).monoBehaviour;
                }
                
                dropdownItem.gameObject.SetActive(true);
                dropdownItem.gameObject.name = option;

                if (i < customLabel.Length)
                    dropdownItem.label.text = customLabel[i];
                else
                    dropdownItem.label.text = option;

                if (i == value)
                    dropdownItem.toggle.isOn = true;
                else
                    dropdownItem.toggle.isOn = false;

                dropdownItem.toggle.interactable = true;

                RendererManager.Refresh(dropdownItem.refreshableObjects, false);

                dropdownItems.Add(dropdownItem);
            }
            invokeLock = false;

            _isShow = true;
        }

        public void Hide()
        {
            if (!isShow)
                return;

            UIManager.BackEventRemove(Hide, true);
            UIManager.homeEvent -= Hide;

            mouseDrag = false;

            _isShow = false;
            listSetSizeAsTargetRectTransform.enabled = false;

            for (int i = 0; i < dropdownItems.Count; i++)
                dropdownItems[i].toggle.interactable = false;
        }

        public void OnValueChangedInvoke(DropdownItem dropdownItem)
        {
            if (!invokeLock)
            {
                if (dropdownItem.toggle.isOn)
                {
                    value = dropdownItem.transform.GetSiblingIndex() - 1;
                    label.text = dropdownItem.label.text;
                }

                onValueChanged.Invoke();
                Hide();
            }
        }

        public void OnPointerEnter(PointerEventData eventData) => pointer = true;

        public void OnPointerExit(PointerEventData eventData) => pointer = false;
    }
}