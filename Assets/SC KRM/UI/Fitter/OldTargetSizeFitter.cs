using System;
using UnityEngine;

namespace SCKRM.UI
{
    [ExecuteAlways]
    [AddComponentMenu("SC KRM/UI/Size Fitter/Old Target Size Fitter")]
    [RequireComponent(typeof(RectTransform))]
    [Obsolete("Use TargetSizeFitter instead")]
    public sealed class OldTargetSizeFitter : UIAniLayoutBase
    {
        [SerializeField] RectTransform _targetRectTransform;
        public RectTransform targetRectTransform { get => _targetRectTransform; set => _targetRectTransform = value; }

        [SerializeField] bool _xSize = false;
        public bool xSize { get => _xSize; set => _xSize = value; }
        [SerializeField] bool _ySize = false;
        public bool ySize { get => _ySize; set => _ySize = value; }

        [SerializeField] Vector2 _offset = Vector2.zero;
        public Vector2 offset { get => _offset; set => _offset = value; }

        [SerializeField, Min(0)] Vector2 _min = Vector2.zero;
        public Vector2 min { get => _min; set => _min = value; }
        [SerializeField, Min(0)] Vector2 _max = Vector2.zero;
        public Vector2 max { get => _max; set => _max = value; }

        [SerializeField] bool _reversal = false;
        public bool reversal { get => _reversal; set => _reversal = value; }



        DrivenRectTransformTracker tracker;


        //protected override void OnEnable() => tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDelta);
        protected override void OnDisable()
        {
            if (!Kernel.isPlaying)
                tracker.Clear();
        }

        public override void LayoutRefresh()
        {
            if (targetRectTransform == null)
                return;

            Vector2 targetSize = targetRectTransform.rect.size;
            size = new Vector2(targetSize.x * targetRectTransform.localScale.x, targetSize.y * targetRectTransform.localScale.y) + offset;
            if (max.x <= 0)
                size.x = size.x.Clamp(min.x);
            else
                size.x = size.x.Clamp(min.x, max.x);
            if (max.y <= 0)
                size.y = size.y.Clamp(min.y);
            else
                size.y = size.y.Clamp(min.y, max.y);

            if (reversal)
                size = parentRectTransform.rect.size - size;
        }

        Vector2 size;
        public override void SizeUpdate(bool useAni = true)
        {
            if (!Kernel.isPlaying)
            {
                tracker.Clear();

                if (xSize)
                    tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaX);
                if (ySize)
                    tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaY);
            }

            if (!lerp || !useAni || !Kernel.isPlaying)
            {
                Rect rect = rectTransform.rect;
                Vector2 size = rect.size;
                if (xSize && !ySize)
                    size = new Vector2(this.size.x, rect.size.y);
                else if (!xSize && ySize)
                    size = new Vector2(rect.size.x, this.size.y);
                else if (xSize && ySize)
                    size = this.size;

                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            }
            else
            {
                Rect rect = rectTransform.rect;
                Vector2 size = rect.size;
                if (xSize && !ySize)
                    size = rect.size.Lerp(new Vector2(this.size.x, rect.size.y), lerpValue * Kernel.fpsUnscaledSmoothDeltaTime);
                else if (!xSize && ySize)
                    size = rect.size.Lerp(new Vector2(rect.size.x, this.size.y), lerpValue * Kernel.fpsUnscaledSmoothDeltaTime);
                else if (xSize && ySize)
                    size = rect.size.Lerp(this.size, lerpValue * Kernel.fpsUnscaledSmoothDeltaTime);

                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            }
        }
    }
}