using Cysharp.Threading.Tasks;
using SCKRM.ProjectSetting;
using SCKRM.Renderer;
using SCKRM.Resource;
using SCKRM.SaveLoad;
using SCKRM.Splash;
using SCKRM.Threads;
using SCKRM.UI;
using SCKRM.UI.StatusBar;
using System;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.SceneManagement;

namespace SCKRM
{
    public static class InitialLoadManager
    {
        public static bool isInitialLoadStart { get; private set; } = false;
        public static bool isSettingLoadEnd { get; private set; } = false;
        public static bool isInitialLoadEnd { get; private set; } = false;

        public static event Action initialLoadStart;
        public static event Action initialLoadEnd;
        public static event Action initialLoadEndSceneMove;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static async UniTaskVoid InitialLoad()
        {
            isInitialLoadStart = false;
            isSettingLoadEnd = false;
            isInitialLoadEnd = false;

            try
            {
                //이 함수는 어떠한 경우에도 메인스레드가 아닌 스레드에서 실행되면 안됩니다
                if (!ThreadManager.isMainThread)
                    throw new NotMainThreadMethodException(nameof(InitialLoad));
#if UNITY_EDITOR
                //이 함수는 어떠한 경우에도 앱이 플레이중이 아닐때 실행되면 안됩니다
                if (!Application.isPlaying)
                    throw new NotPlayModeMethodException(nameof(InitialLoad));

                if (UnityEditor.EditorSettings.enterPlayModeOptionsEnabled && UnityEditor.EditorSettings.enterPlayModeOptions.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
                {
                    GameObject[] gameObjects = UnityEngine.Object.FindObjectsOfType<GameObject>(true);
                    int length = gameObjects.Length;
                    for (int i = 0; i < length; i++)
                    {
                        GameObject gameObject = gameObjects[i];
                        if (gameObject != null && UnityEditor.PrefabUtility.GetPrefabInstanceStatus(gameObject) == UnityEditor.PrefabInstanceStatus.NotAPrefab)
                            UnityEngine.Object.DestroyImmediate(gameObject);
                    }

                    UnityEditor.EditorApplication.isPlaying = false;
                    throw new Exception("SC KRM은 Disable Domain Reload를 지원 하지 않습니다\nSC KRM does not support Disable Domain Reload");
                }
#endif
                //초기로딩이 시작됬습니다
                isInitialLoadStart = true;
                initialLoadStart?.Invoke();

                PlayerLoopSystem loop = PlayerLoop.GetCurrentPlayerLoop();
                PlayerLoopHelper.Initialize(ref loop);

                StatusBarManager.allowStatusBarShow = false;

                //스레드를 자동 삭제해주는 함수를 작동시킵니다
                ThreadManager.ThreadAutoRemove().Forget();

#if UNITY_EDITOR
                //에디터에선 스플래시 씬에서 시작하지 않기 때문에
                //시작한 씬의 인덱스를 구하고
                //인덱스가 0번이 아니면 스플래시 씬을 로딩합니다
                Scene scene = SceneManager.GetActiveScene();
                int startedSceneIndex = scene.buildIndex;
                if (startedSceneIndex != 0)
                {
                    //씬을 이동하기 전에 Awake가 작동하지 않게 모든 오브젝트를 삭제합니다
                    MonoBehaviour[] gameObjects = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true);
                    for (int i = 0; i < gameObjects.Length; i++)
                        UnityEngine.Object.DestroyImmediate(gameObjects[i]);

                    SceneManager.LoadScene(0);
                }
#endif
                //빌드된곳에선 스플래시 씬에서 시작하기 때문에
                //아무런 조건문 없이 바로 시작합니다

                //다른 스레드에서 이 값을 설정하기 전에
                //미리 설정합니다
                //(참고: 이 변수는 프로퍼티고 변수가 비어있다면 Application를 호출합니다)
                {
                    _ = Kernel.dataPath;
                    _ = Kernel.persistentDataPath;
                    _ = Kernel.temporaryCachePath;
                    _ = Kernel.saveDataPath;
                    _ = Kernel.resourcePackPath;

                    _ = Kernel.companyName;
                    _ = Kernel.productName;

                    _ = Kernel.version;
                    _ = Kernel.unityVersion;
                }

                Debug.Log("Kernel: Waiting for settings to load...");
                {
                    //세이브 데이터의 기본값과 변수들을 다른 스레드에서 로딩합니다
                    if (await UniTask.RunOnThreadPool(Initialize, cancellationToken: AsyncTaskManager.cancelToken).SuppressCancellationThrow())
                        return;

                    //세이브 데이터를 다른 스레드에서 로딩합니다
                    if (await UniTask.RunOnThreadPool(() => SaveLoadManager.LoadAll(ProjectSettingManager.projectSettingSLCList, Kernel.projectSettingPath), cancellationToken: AsyncTaskManager.cancelToken).SuppressCancellationThrow())
                        return;

                    //세이브 데이터를 다른 스레드에서 로딩합니다
                    if (await UniTask.RunOnThreadPool(() => SaveLoadManager.LoadAll(SaveLoadManager.generalSLCList, Kernel.saveDataPath), cancellationToken: AsyncTaskManager.cancelToken).SuppressCancellationThrow())
                        return;

                    void Initialize()
                    {
                        SaveLoadManager.InitializeAll<GeneralSaveLoadAttribute>(out SaveLoadClass[] saveLoadClass);
#pragma warning disable CS0618 // 형식 또는 멤버는 사용되지 않습니다.
                        SaveLoadManager.generalSLCList = saveLoadClass;
#pragma warning restore CS0618 // 형식 또는 멤버는 사용되지 않습니다.

                        SaveLoadManager.InitializeAll<ProjectSettingSaveLoadAttribute>(out saveLoadClass);
#pragma warning disable CS0618 // 형식 또는 멤버는 사용되지 않습니다.
                        ProjectSettingManager.projectSettingSLCList = saveLoadClass;
#pragma warning restore CS0618 // 형식 또는 멤버는 사용되지 않습니다.
                    }
                }

                isSettingLoadEnd = true;

                {
                    //리소스를 로딩합니다
                    Debug.Log("Kernel: Waiting for resource to load...");
                    await ResourceManager.ResourceRefresh(true);

#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        return;
#endif
                }

                {
                    //초기 로딩이 끝났습니다
                    isInitialLoadEnd = true;
                    initialLoadEnd?.Invoke();

                    //리소스를 로딩했으니 모든 렌더러를 전부 재렌더링합니다
                    RendererManager.AllRerender();

                    Debug.Log("Kernel: Initial loading finished!");
                }

#if UNITY_EDITOR
                if (startedSceneIndex == 0)
#endif
                {
                    //씬 애니메이션이 끝날때까지 기다립니다
                    Debug.Log("Kernel: Waiting for scene animation...");
                    if (await UniTask.WaitUntil(() => !SplashScreen.isAniPlayed, cancellationToken: AsyncTaskManager.cancelToken).SuppressCancellationThrow())
                        return;
                }
#if UNITY_EDITOR
                else
                    SplashScreen.isAniPlayed = false;
#endif

                StatusBarManager.allowStatusBarShow = true;

                //씬이 바뀌었을때 렌더러를 재 렌더링해야하기때문에 이벤트를 걸어줍니다
                SceneManager.sceneLoaded += (Scene scene, LoadSceneMode loadSceneMode) => RendererManager.AllRerender();

                //GC를 호출합니다
                GC.Collect();

#if UNITY_EDITOR
                //씬을 이동합니다
                if (startedSceneIndex != 0)
                    SceneManager.LoadScene(startedSceneIndex);
                else
                    SceneManager.LoadScene(1);
#else
                    SceneManager.LoadScene(1);
#endif

                //씬을 이동했으면 이벤트를 호출합니다
                initialLoadEndSceneMove?.Invoke();
            }
            catch (Exception e)
            {
                //예외를 발견하면 앱을 강제 종료합니다
                //에디터 상태라면 플레이 모드를 종료합니다
                Debug.LogException(e);

                if (!isInitialLoadEnd)
                {
                    Debug.LogError("Kernel: Initial loading failed");

                    if (isInitialLoadStart)
                    {
                        if (await UniTask.WaitUntil(() => UIManager.instance != null, PlayerLoopTiming.Update, AsyncTaskManager.cancelToken).SuppressCancellationThrow())
                            return;

                        UnityEngine.Object.Instantiate(UIManager.instance.exceptionText, UIManager.instance.kernelCanvas.transform).text = "Kernel: Initial loading failed\n\n" + e.GetType().Name + ": " + e.Message + "\n\n" + e.StackTrace.Substring(5);
                    }
                }
            }
        }
    }
}
