using SCKRM.Scene;
using System;
using UnityEngine;

namespace SCKRM.Object
{
    [WikiDescription("오브젝트 풀링 인터페이스")]
    public interface IObjectPooling : IRemoveable
    {
        string objectKey { get; set; }
        bool isActived { get; [Obsolete("It is managed by the ObjectPoolingSystem class. Please do not touch it.")] internal set; }

        bool disableCreation { get; set; }

        IRefreshable[] refreshableObjects { get; }

        Action removed { get; set; }

        void OnCreate();

        bool IsDestroyed();

        void ActiveSceneChanged();

        [WikiDescription("오브젝트를 생성할때의 기본 스크립트")]
        public static void OnCreateDefault(Transform transform, IObjectPooling objectPooling)
        {
            transform.gameObject.name = objectPooling.objectKey;

            transform.localPosition = Vector3.zero;

            transform.localEulerAngles = Vector3.zero;
            transform.localScale = Vector3.one;

            SceneManager.activeSceneChanged += objectPooling.ActiveSceneChanged;
        }

        [WikiDescription("오브젝트를 삭제할때의 기본 스크립트")]
        public static bool RemoveDefault(MonoBehaviour monoBehaviour, IObjectPooling objectPooling)
        {
            if (!objectPooling.isActived)
                return false;
            if (!Kernel.isPlaying)
                return false;

            objectPooling.removed?.Invoke();
            objectPooling.removed = null;

            ObjectPoolingSystem.ObjectRemove(objectPooling.objectKey, monoBehaviour, objectPooling);
            monoBehaviour.name = objectPooling.objectKey;

            monoBehaviour.transform.localPosition = Vector3.zero;

            monoBehaviour.transform.localEulerAngles = Vector3.zero;
            monoBehaviour.transform.localScale = Vector3.one;

            monoBehaviour.StopAllCoroutines();

            SceneManager.activeSceneChanged -= objectPooling.ActiveSceneChanged;
            return true;
        }

        [WikiIgnore]
        public static bool RemoveDefault(UI.UIBase ui, IObjectPooling objectPooling)
        {
            if (!objectPooling.isActived)
                return false;
            if (!Kernel.isPlaying)
                return false;

            objectPooling.removed?.Invoke();
            objectPooling.removed = null;

            ObjectPoolingSystem.ObjectRemove(objectPooling.objectKey, ui, objectPooling);

            ui.name = objectPooling.objectKey;

            ui.rectTransform.anchoredPosition = Vector3.zero;

            ui.rectTransform.localEulerAngles = Vector3.zero;
            ui.rectTransform.localScale = Vector3.one;

            ui.StopAllCoroutines();

            SceneManager.activeSceneChanged -= objectPooling.ActiveSceneChanged;
            return true;
        }
    }

    [WikiDescription("오브젝트 풀링으로 생성된 오브젝트를 관리하는 클래스 입니다")]
    [AddComponentMenu("SC KRM/Object/Object Pooling")]
    public class ObjectPoolingBase : MonoBehaviour, IObjectPooling
    {
        [WikiDescription("오브젝트 키")] public string objectKey { get; set; }

        [WikiDescription("삭제 여부")] public bool isRemoved => !isActived;

        [WikiDescription("활성화 여부")] public bool isActived { get; private set; }
        bool IObjectPooling.isActived { get => isActived; set => isActived = value; }

        [WikiDescription("생성 비활성화 여부")] public bool disableCreation { get; protected set; }
        bool IObjectPooling.disableCreation { get => disableCreation; set => disableCreation = value; }



        IRefreshable[] _refreshableObjects;
        [WikiDescription("새로고침 가능한 오브젝트들을 가져옵니다")] public IRefreshable[] refreshableObjects => _refreshableObjects = this.GetComponentsInChildrenFieldSave(_refreshableObjects, true);

        Action IObjectPooling.removed { get => _removed; set => _removed = value; }
        public event Action removed { add => _removed += value; remove => _removed -= value; } Action _removed = null;



        /// <summary>
        /// Please put base.OnCreate() when overriding
        /// </summary>
        public virtual void OnCreate() => IObjectPooling.OnCreateDefault(transform, this);

        /// <summary>
        /// Please put base.Remove() when overriding
        /// </summary>
        [WikiDescription("오브젝트 삭제")]
        public virtual void Remove() => IObjectPooling.RemoveDefault(this, this);

        public bool IsDestroyed() => this == null;

        /// <summary>
        /// Please put base.ActiveSceneChanged() when overriding
        /// </summary>
        public virtual void ActiveSceneChanged()
        {
            if (!isRemoved && gameObject.scene.name != "DontDestroyOnLoad")
                Remove();
        }

        /// <summary>
        /// Please put base.OnDestroy() when overriding
        /// </summary>
        protected virtual void OnDestroy() => SceneManager.activeSceneChanged -= ActiveSceneChanged;
    }
}
