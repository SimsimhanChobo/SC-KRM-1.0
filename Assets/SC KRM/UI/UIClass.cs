using SCKRM.Object;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SCKRM.UI
{
    public class UI : UIBehaviour
    {
        [SerializeField] RectTransform _rectTransform; public RectTransform rectTransform => _rectTransform = this.GetComponentFieldSave(_rectTransform);
        [SerializeField] Graphic _graphic; public Graphic graphic => _graphic = this.GetComponentFieldSave(_graphic, ComponentTool.GetComponentMode.none);
    }

    public class ManagerUI<T> : UIBehaviour where T : MonoBehaviour
    {
        public static T instance { get; private set; }



        protected static bool SingletonCheck(T manager)
        {
            if (instance != null && instance != manager)
            {
                DestroyImmediate(manager.gameObject);
                return false;
            }

            return (instance = manager) == manager;
        }

        [NonSerialized] RectTransform _rectTransform; public RectTransform rectTransform => this.GetComponentFieldSave(_rectTransform);
        [NonSerialized] Graphic _graphic; public Graphic graphic => this.GetComponentFieldSave(_graphic, ComponentTool.GetComponentMode.none);
    }

    public abstract class UILayout : UI
    {
        protected override void Awake()
        {
            onTransformParentChangedMethodLock = true;
            LayoutUpdate();
            onTransformParentChangedMethodLock = false;
        }

        bool onTransformParentChangedMethodLock = false;
        protected override void OnTransformParentChanged()
        {
            if (onTransformParentChangedMethodLock || !isActiveAndEnabled)
                return;

            onTransformParentChangedMethodLock = true;
            LayoutUpdate();
            onTransformParentChangedMethodLock = false;
        }

        bool onRectTransformDimensionsChangeMethodLock = false;
        protected override void OnRectTransformDimensionsChange()
        {
            if (onRectTransformDimensionsChangeMethodLock || !isActiveAndEnabled)
                return;

            onRectTransformDimensionsChangeMethodLock = true;
            LayoutUpdate();
            onRectTransformDimensionsChangeMethodLock = false;
        }

        public abstract void LayoutUpdate();
    }

    public class UIAni : UI
    {
        [SerializeField] bool _lerp = true;
        public bool lerp { get => _lerp; set => _lerp = value; }
        [SerializeField, Range(0, 1)] float _lerpValue = 0.2f;
        public float lerpValue { get => _lerpValue; set => _lerpValue = value; }
    }

    public abstract class UIAniLayout : UIAni
    {
        //protected override void OnEnable() => update();

        //protected override void OnValidate() => update();

        protected override void Awake()
        {
            onTransformParentChangedMethodLock = true;
            Update();
            onTransformParentChangedMethodLock = false;
        }

        bool onTransformParentChangedMethodLock = false;
        protected override void OnTransformParentChanged()
        {
            if (onTransformParentChangedMethodLock || (lerp && lerpValue < 1) || !isActiveAndEnabled)
                return;

            onTransformParentChangedMethodLock = true;
            Update();
            onTransformParentChangedMethodLock = false;
        }

        bool onRectTransformDimensionsChangeMethodLock = false;
        protected override void OnRectTransformDimensionsChange()
        {
            if (onRectTransformDimensionsChangeMethodLock || (lerp && lerpValue < 1) || !isActiveAndEnabled)
                return;

            onRectTransformDimensionsChangeMethodLock = true;
            Update();
            onRectTransformDimensionsChangeMethodLock = false;
        }

        /*void update()
        {
#if UNITY_EDITOR
            if (!lerp || !Application.isPlaying)
#else
            if (!lerp)
#endif
            {
                LayoutRefresh();
                SizeUpdate();
            }
            else
            {
                Debug.Log("asdf");
                LayoutRefresh();
            }
        }

        protected virtual void Update()
        {
#if UNITY_EDITOR
            if (lerp && Application.isPlaying)
#else
            if (lerp)
#endif
                SizeUpdate();
        }*/

        protected virtual void Update()
        {
            onRectTransformDimensionsChangeMethodLock = true;

            LayoutRefresh();
            SizeUpdate();

            onRectTransformDimensionsChangeMethodLock = false;
        }

        protected virtual void LayoutRefresh()
        {

        }

        protected abstract void SizeUpdate();
    }

    public class ObjectPoolingUI : ObjectPooling
    {
        [SerializeField] RectTransform _rectTransform; public RectTransform rectTransform => _rectTransform = this.GetComponentFieldSave(_rectTransform);
        [SerializeField] Graphic _graphic; public Graphic graphic => _graphic = this.GetComponentFieldSave(_graphic, ComponentTool.GetComponentMode.none);

        public override void Remove()
        {
            base.Remove();

            if (rectTransform != null)
                rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}