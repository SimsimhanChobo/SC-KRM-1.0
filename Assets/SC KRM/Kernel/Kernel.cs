using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using SCKRM.Tool;
using SCKRM.Input;
using SCKRM.Language;
using SCKRM.ProjectSetting;
using SCKRM.Renderer;
using SCKRM.Resource;
using SCKRM.SaveLoad;
using SCKRM.Sound;
using SCKRM.Splash;
using SCKRM.Threads;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SCKRM.Window;
using System.Threading.Tasks;
using SCKRM.UI.SideBar;
using SCKRM.UI.StatusBar;
using SCKRM.Json;

namespace SCKRM
{
    [AddComponentMenu("커널/커널")]
    public sealed class Kernel : Manager<Kernel>
    {
        [ProjectSetting]
        public sealed class Data
        {
            [JsonProperty] public static float standardFPS { get; set; } = 60;



            [JsonProperty] public static int notFocusFpsLimit { get; set; } = 30;
            [JsonProperty] public static int afkFpsLimit { get; set; } = 30;

            [JsonProperty] public static float afkTimerLimit { get; set; } = 60;



            [JsonProperty] public static string splashScreenPath { get; set; } = "Assets/SC KRM/Splash Screen";
            [JsonProperty] public static string splashScreenName { get; set; } = "Splash Screen";
        }

        [SaveLoad("default")]
        public sealed class SaveData
        {
            [JsonProperty] public static JColor systemColor { get; set; } = JColor.one;

            [JsonProperty] public static int mainVolume { get; set; } = 100;
            [JsonProperty] public static int bgmVolume { get; set; } = 100;
            [JsonProperty] public static int soundVolume { get; set; } = 100;

            [JsonProperty] public static bool vSync { get; set; } = true;
            [JsonProperty] public static int fpsLimit { get; set; } = 300;
            [JsonProperty] public static float guiSize { get; set; } = 1;
            [JsonProperty] public static float fixedGuiSize { get; set; } = 1;
            [JsonProperty] public static bool fixedGuiSizeEnable { get; set; } = true;
        }

        public static float fps { get; private set; } = 60;

        public static float deltaTime { get; private set; } = fps60second;
        public static float fpsDeltaTime { get; private set; } = 1;
        public static float unscaledDeltaTime { get; private set; } = fps60second;
        public static float fpsUnscaledDeltaTime { get; private set; } = 1;

        public const float fps60second = 1f / 60f;
        
        static string _dataPath = "";
        /// <summary>
        /// Application.dataPath
        /// </summary>
        public static string dataPath
        {
            get
            {
                if (_dataPath != "")
                    return _dataPath;
                else
                    return _dataPath = Application.dataPath;
            }
        }

        /// <summary>
        /// Application.streamingAssetsPath
        /// </summary>
        public static string streamingAssetsPath { get; } = Application.streamingAssetsPath;

        static string _persistentDataPath = "";
        /// <summary>
        /// Application.persistentDataPath
        /// </summary>
        public static string persistentDataPath
        {
            get
            {
                if (_persistentDataPath != "")
                    return _persistentDataPath;
                else
                    return _persistentDataPath = Application.persistentDataPath;
            }
        }

        static string _saveDataPath = "";
        /// <summary>
        /// Kernel.persistentDataPath + "/Save Data"
        /// </summary>
        public static string saveDataPath
        {
            get
            {
                if (_saveDataPath != "")
                    return _saveDataPath;
                else
                    return _saveDataPath = persistentDataPath + "/Save Data";
            }
        }

        /// <summary>
        /// Kernel.streamingAssetsPath + "/projectSettings"
        /// </summary>
        public static string projectSettingPath { get; } = streamingAssetsPath + "/projectSettings";



        static string _companyName = "";
        public static string companyName
        {
            get
            {
                if (_companyName != "")
                    return _companyName;
                else
                    return _companyName = Application.companyName;
            }
        }

        static string _productName = "";
        public static string productName
        {
            get
            {
                if (_productName != "")
                    return _productName;
                else
                    return _productName = Application.productName;
            }
        }

        static string _version = "";
        public static string version
        {
            get
            {
                if (_version != "")
                    return _version;
                else
                    return _version = Application.version;
            }
        }



        static string _unityVersion = "";
        public static string unityVersion
        {
            get
            {
                if (_version != "")
                    return _unityVersion;
                else
                    return _unityVersion = Application.unityVersion;
            }
        }



        public static RuntimePlatform platform { get; } = Application.platform;



        public static bool isAFK { get; private set; } = false;
        public static float afkTimer { get; private set; } = 0;






        public static float gameSpeed { get; set; } = 1;
        public static float guiSize { get; private set; } = 1;

        public static bool isInitialLoadStart { get; private set; } = false;
        public static bool isInitialLoadEnd { get; private set; } = false;



        [SerializeField] Image _splashScreenBackground;
        public Image splashScreenBackground => _splashScreenBackground;

        void Awake()
        {
            if (SingletonCheck(this))
            {
                DontDestroyOnLoad(instance);
                InitialLoad().Forget();
            }
        }

#if !UNITY_EDITOR
        IEnumerator Start()
        {
            while (true)
            {
                if (isInitialLoadEnd && InputManager.GetKey("kernel.full_screen", InputType.Down, "all"))
                {
                    if (Screen.fullScreen)
                        Screen.SetResolution((int)(Screen.currentResolution.width / 1.5f), (int)(Screen.currentResolution.height / 1.5f), false);
                    else
                    {
                        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, false);
                        yield return new WaitForEndOfFrame();
                        yield return new WaitForEndOfFrame();
                        yield return new WaitForEndOfFrame();
                        yield return new WaitForEndOfFrame();
                        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
                    }
                }
                yield return null;
            }
        }
#endif

        static int tempYear;
        static int tempMonth;
        static int tempDay;
        void Update()
        {
            //유니티의 내장 변수들은 테스트 결과, 약간의 성능을 더 먹는것으로 확인되었기 때문에
            //이렇게 관리 스크립트가 변수를 할당하고 다른 스크립트가 그 변수를 가져오는것이 성능에 더 도움 되는것을 확인하였습니다
            fps = 1f / deltaTime;
            deltaTime = Time.deltaTime;
            fpsDeltaTime = deltaTime * Data.standardFPS;
            unscaledDeltaTime = Time.unscaledDeltaTime;
            fpsUnscaledDeltaTime = unscaledDeltaTime * Data.standardFPS;

            //AFK
            //초기 로딩이 끝났고 아무 키를 눌렀을때 AFK 타이머를 0으로 설정합니다
            if (isInitialLoadEnd && InputManager.GetAnyKeyDown("all"))
                afkTimer = 0;

            //AFK 타이머가 프로젝트에서 설정한 타이머를 넘었다면 AFK 모드를 활성화합니다
            if (afkTimer >= Data.afkTimerLimit)
                isAFK = true;
            else //아니라면 타이머를 계속 증가시키고 AFK 모드를 비활성화 합니다
            {
                isAFK = false;
                afkTimer += unscaledDeltaTime;
            }

            //게임 속도를 0에서 100 사이로 정하고, 타임 스케일을 게임 속도로 정합니다
            gameSpeed = gameSpeed.Clamp(0, 100);
            Time.timeScale = gameSpeed;

            //기념일
            //초기로딩이 끝나야 알림을 띄울수 있으니 조건을 걸어둡니다
            //최적화를 위해 년, 월, 일이 변경되어야 실행됩니다
            DateTime dateTime = DateTime.Now;
            if (isInitialLoadEnd && (tempYear != dateTime.Year || tempMonth != dateTime.Month || tempDay != dateTime.Day))
            {
                //음력 날짜를 정합니다
                DateTime dateTimeLunar = dateTime.ToLunarDate();

                //8월 7일이라면...
                if (dateTime.Month == 8 && dateTime.Day == 7)
                    NoticeManager.Notice("notice.kurumi_chan.birthday.title", "notice.kurumi_chan.birthday.description");
                else if (dateTime.Month == 2 && dateTime.Day == 9) //2월 9일이라면...
                    NoticeManager.Notice("notice.onell0.birthday.title", "notice.onell0.birthday.description", "%value%", (dateTime.Year - 2010).ToString());
                else if (dateTimeLunar.Month == 1 && dateTimeLunar.Day == 1) //음력으로 1월 1일이라면...
                    NoticeManager.Notice("notice.korean_new_year.title", "notice.korean_new_year.description");

                tempYear = dateTime.Year;
                tempMonth = dateTime.Month;
                tempDay = dateTime.Day;
            }
        }

        //변수 업데이트
        void LateUpdate()
        {
            //현제 해상도의 가로랑 1920을 나눠서 모든 해상도에도 가로 픽셀 크기는 똑같이 유지되게 함
            float defaultGuiSize = (float)Screen.width / 1920;

            //변수들의 최소, 최대 수치를 지정합니다
            SaveData.mainVolume = SaveData.mainVolume.Clamp(0, 200);
            SaveData.bgmVolume = SaveData.bgmVolume.Clamp(0, 200);
            SaveData.soundVolume = SaveData.soundVolume.Clamp(0, 200);

            SaveData.fpsLimit = SaveData.fpsLimit.Clamp(30);
            SaveData.fixedGuiSize = SaveData.fixedGuiSize.Clamp(defaultGuiSize * 0.5f, defaultGuiSize * 4f);
            SaveData.guiSize = SaveData.guiSize.Clamp(0.5f, 4);
            Data.notFocusFpsLimit = Data.notFocusFpsLimit.Clamp(1);
            Data.afkFpsLimit = Data.afkFpsLimit.Clamp(1);
            Data.afkTimerLimit = Data.afkTimerLimit.Clamp(0);

            //GUI 크기 설정
            //고정 GUI 크기가 꺼져있다면 화면 크기에 따라 유동적으로 GUI 크기가 변경됩니다
            if (!SaveData.fixedGuiSizeEnable)
                guiSize = SaveData.guiSize * defaultGuiSize;
            else //고정 GUI 크기가 켜져있다면 GUI 크기를 고정시킵니다
                guiSize = SaveData.fixedGuiSize;

            //FPS Limit
            //AFK 상태가 아니고 앱이 포커스 상태이거나 에디터 상태라면 사용자가 지정한 프레임으로 고정시킵니다
            if (!isAFK && (Application.isFocused || Application.isEditor))
                Application.targetFrameRate = SaveData.fpsLimit;
            else if (!isAFK && !Application.isFocused) //AFK 상태가 아니고 앱이 포커스 상태가 아니라면 프로젝트에서 설정한 포커스가 아닌 프레임으로 고정시킵니다
                Application.targetFrameRate = Data.notFocusFpsLimit;
            else //AFK 상태라면 프로젝트에서 설정한 AFK 프레임으로 고정시킵니다
                Application.targetFrameRate = Data.afkFpsLimit;

            //수직동기화
            if (!SaveData.vSync)
                QualitySettings.vSyncCount = 0;
            else
                QualitySettings.vSyncCount = 1;

            //볼륨을 사용자가 설정한 볼륨으로 조정시킵니다. 사용자가 설정한 볼륨은 int 0 ~ 200 이기 때문에 0.01을 곱해주어야 하고,
            //100 ~ 200 볼륨이 먹혀야하기 때문에 0.5로 볼륨을 낮춰야하기 때문에 0.005를 곱합니다
            AudioListener.volume = SaveData.mainVolume * 0.005f;
        }

        public static event Action InitialLoadStart = delegate { };
        public static event Action InitialLoadEnd = delegate { };
        public static event Action InitialLoadEndSceneMove = delegate { };
        async UniTaskVoid InitialLoad()
        {
            //이미 초기로딩이 시작되었으면 더 이상 초기로딩을 진행하면 안되기 때문에 조건문을 걸어줍니다
            if (!isInitialLoadStart)
            {
                try
                {
                    //이 함수는 어떠한 경우에도 메인스레드에서 실행되면 안됩니다
                    if (!ThreadManager.isMainThread)
                        throw new NotMainThreadMethodException(nameof(InitialLoad));
#if UNITY_EDITOR
                    //이 함수는 어떠한 경우에도 앱이 플레이중이 아닐때 실행되면 안됩니다
                    if (!Application.isPlaying)
                        throw new NotPlayModeMethodException(nameof(InitialLoad));
#endif
                    //초기로딩이 시작됬습니다
                    isInitialLoadStart = true;
                    InitialLoadStart();

                    StatusBarManager.allowStatusBarShow = false;

                    //스플래시 배경화면을 킵니다, 나중에 페이드 아웃을 하기위해 있습니다
                    splashScreenBackground.gameObject.SetActive(true);
                    splashScreenBackground.color = new Color(splashScreenBackground.color.r, splashScreenBackground.color.g, splashScreenBackground.color.b, 1);

                    //ThreadManager.Create(() => ThreadManager.ThreadAutoRemove(true), "notice.running_task.thread_auto_remove.name", "notice.running_task.thread_auto_remove.info", true);
                    //스레드를 자동 삭제해주는 함수를 작동시킵니다
                    ThreadManager.ThreadAutoRemove().Forget();

#if UNITY_EDITOR
                    //에디터에선 스플래시 씬에서 시작하지 않기 때문에
                    //시작한 씬의 인덱스를 구하고
                    //인덱스가 0번이 아니면 스플래시 씬을 로딩합니다
                    Scene scene = SceneManager.GetActiveScene();
                    int startedSceneIndex = scene.buildIndex;
                    if (startedSceneIndex != 0)
                        SceneManager.LoadScene(0);
#endif
                    //빌드된곳에선 스플래시 씬에서 시작하기 때문에
                    //아무런 조건문 없이 바로 시작합니다

                    //다른 스레드에서 이 값을 설정하기 전에
                    //미리 설정합니다
                    //(참고: 이 변수는 프로퍼티고 변수가 비어있다면 Application를 호출합니다)
                    {
                        _ = dataPath;
                        _ = persistentDataPath;
                        _ = saveDataPath;

                        _ = companyName;
                        _ = productName;

                        _ = version;
                        _ = unityVersion;
                    }

                    Debug.Log("Kernel: Waiting for settings to load...");
                    {
                        //프로젝트 설정을 다른 스레드에서 로딩합니다
                        ThreadMetaData projectSettingLoad = ThreadManager.Create(ProjectSettingManager.Load, "Project Setting Load");
                        await UniTask.WaitUntil(() => projectSettingLoad.thread == null, PlayerLoopTiming.Initialization);

                        //세이브 데이터의 기본값과 변수들을 다른 스레드에서 로딩합니다
                        projectSettingLoad = ThreadManager.Create(SaveLoadManager.VariableInfoLoad, "Save Data Load");
                        await UniTask.WaitUntil(() => projectSettingLoad.thread == null, PlayerLoopTiming.Initialization);

                        //세이브 데이터를 다른 스레드에서 로딩합니다
                        projectSettingLoad = ThreadManager.Create(SaveLoadManager.Load, "Save Data Load");
                        await UniTask.WaitUntil(() => projectSettingLoad.thread == null, PlayerLoopTiming.Initialization);
                    }

                    {
                        //리소스를 로딩합니다
                        Debug.Log("Kernel: Waiting for resource to load...");
                        await ResourceManager.ResourceRefresh();
                    }
                    
                    {
                        //초기 로딩이 끝났습니다
                        InitialLoadEnd();
                        isInitialLoadEnd = true;

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
                        await UniTask.WaitUntil(() => !SplashScreen.isAniPlayed, PlayerLoopTiming.Initialization);
                    }

                    StatusBarManager.allowStatusBarShow = true;

                    //씬이 바뀌었을때 렌더러를 재 렌더링해야하기때문에 이벤트를 걸어줍니다
                    SceneManager.sceneLoaded += LoadedSceneEvent;

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
                    InitialLoadEndSceneMove();

                    //씬이 이동하고 나서 잠깐 렉이 있기 때문에, 애니메이션이 제대로 재생될려면 딜레이를 걸어줘야합니다
                    await UniTask.DelayFrame(3);

                    if (splashScreenBackground != null)
                    {
                        while (splashScreenBackground.color.a > 0)
                        {
                            Color color = splashScreenBackground.color;
                            splashScreenBackground.color = new Color(color.r, color.g, color.b, color.a.MoveTowards(0, 0.05f * fpsUnscaledDeltaTime));

                            await UniTask.DelayFrame(1, PlayerLoopTiming.Initialization);
                        }

                        splashScreenBackground.gameObject.SetActive(false);
                    }
                }
                catch (Exception e)
                {
                    //예외를 발견하면 앱을 강제 종료합니다
                    //에디터 상태라면 플레이 모드를 종료합니다
                    Debug.LogException(e);

                    if (!isInitialLoadEnd)
                    {
                        Debug.LogError("Kernel: Initial loading failed");
#if UNITY_EDITOR
                        if (Application.isPlaying)
                        {
                            //플레이 모드가 바로 종료되지 않기 때문에
                            //다른 예외가 날 가능성이 있어서 먼저 모든 게임 오브젝트를 지웁니다
                            GameObject[] gameObjects = FindObjectsOfType<GameObject>(true);
                            int length = gameObjects.Length;
                            for (int i = 0; i < length; i++)
                                DestroyImmediate(gameObjects[i]);

                            UnityEditor.EditorApplication.isPlaying = false;
                        }
                        else
                        {
                            //예외가 났는데 플레이 모드가 종료된 상태면
                            //십중팔구로 초기로딩이 끝나지 않은상태에서
                            //플레이 모드를 종료한거기 때문에 경고만 띄워줍니다
                            Debug.LogWarning("Kernel: Do not exit play mode during initial loading");
                        }
#else

                        WindowManager.MessageBox(e.GetType().Name + ": " + e.Message + "\n\n" + e.StackTrace.Substring(5), "Kernel: Initial loading failed", WindowManager.MessageBoxButtons.OK, WindowManager.MessageBoxIcon.Error);
                        Application.Quit(1);
#endif
                    }
                }
            }
        }

        static void LoadedSceneEvent(Scene scene, LoadSceneMode mode) => RendererManager.AllRerender();


        public static event Action AllRefreshStart;
        public static event Action AllRefreshEnd;
        public static async UniTaskVoid AllRefresh(bool onlyText = false)
        {
            if (!ThreadManager.isMainThread)
                throw new NotMainThreadMethodException(nameof(AllRefresh));
#if UNITY_EDITOR
            if (!Application.isPlaying)
                throw new NotPlayModeMethodException(nameof(AllRefresh));
#endif
            AllRefreshStart?.Invoke();

            if (onlyText)
                RendererManager.AllTextRerender();
            else
            {
/*#if !UNITY_EDITOR
                if (SoundManager.soundList.Count > 0)
                {
#if UNITY_STANDALONE_WIN
                    string text = LanguageManager.TextLoad("kernel.allrefresh.warning");
                    string caption = LanguageManager.TextLoad("gui.warning");
                    WindowManager.DialogResult dialogResult = WindowManager.MessageBox(text, caption, WindowManager.MessageBoxButtons.OKCancel, WindowManager.MessageBoxIcon.Warning);
                    if (dialogResult != WindowManager.DialogResult.OK)
                        return;
#else
                Debug.LogError(LanguageManager.TextLoad("kernel.allrefresh.error"));
                return;
#endif
                }
#endif*/
                if (!ResourceManager.isResourceRefesh)
                {
                    await ResourceManager.ResourceRefresh();
                    RendererManager.AllRerender();

                    SoundManager.SoundRefresh();
                    ResourceManager.AudioGarbageRemoval();
                }
            }
            
            GC.Collect();
            AllRefreshEnd?.Invoke();
        }

        void OnApplicationQuit()
        {
            ThreadManager.AllThreadRemove();

            if (isInitialLoadEnd)
                SaveLoadManager.Save();
        }
    }


    public class NotPlayModeMethodException : Exception
    {
        /// <summary>
        /// It is not possible to use this function when not in play mode.
        /// 플레이 모드가 아닐때 이 함수를 사용하는건 불가능합니다.
        /// </summary>
        public NotPlayModeMethodException() : base("It is not possible to use this function when not in play mode.\n플레이 모드가 아닐때 이 함수를 사용하는건 불가능합니다") { }

        /// <summary>
        /// It is not possible to use {method} functions when not in play mode.
        /// 플레이 모드가 아닐때 이 함수를 사용하는건 불가능합니다.
        /// </summary>
        public NotPlayModeMethodException(string method) : base($"It is not possible to use {method} functions when not in play mode.\n플레이 모드가 아닐때 {method} 함수를 사용하는건 불가능합니다") { }
    }



    public class NotInitialLoadEndMethodException : Exception
    {
        /// <summary>
        /// Initial loading was not finished, but I tried to use a function that needs loading
        /// 초기 로딩이 안끝났는데 로딩이 필요한 함수를 사용하려 했습니다
        /// </summary>
        public NotInitialLoadEndMethodException() : base("Initial loading was not finished, but I tried to use a function that needs loading\n초기 로딩이 안끝났는데 로딩이 필요한 함수를 사용하려 했습니다") { }

        /// <summary>
        /// Initial loading was not finished, but I tried to use a {method} function that needs loading
        /// 초기 로딩이 안끝났는데 로딩이 필요한 {method} 함수를 사용하려 했습니다
        /// </summary>
        public NotInitialLoadEndMethodException(string method) : base($"Initial loading was not finished, but I tried to use a {method} function that needs loading\n초기 로딩이 안끝났는데 로딩이 필요한 {method} 함수를 사용하려 했습니다") { }
    }



    public class NullResourceObjectException : Exception
    {
        /// <summary>
        /// No object in resource folder
        /// 리소스 폴더에 오브젝트가 없습니다
        /// </summary>
        public NullResourceObjectException() : base("No object in resource folder\n리소스 폴더에 오브젝트가 없습니다") { }

        /// <summary>
        /// Object {objectName} does not exist in resource folder
        /// 리소스 폴더에 {objectName} 오브젝트가 없습니다
        /// </summary>
        public NullResourceObjectException(string objectName) : base($"Object {objectName} does not exist in resource folder\n리소스 폴더에 {objectName} 오브젝트가 없습니다") { }
    }

    public class NullSceneException : Exception
    {
        /// <summary>
        /// no scene
        /// 씬이 없습니다
        /// </summary>
        public NullSceneException() : base("no scene\n씬이 없습니다") { }

        /// <summary>
        /// {sceneName} no scene
        /// {sceneName} 씬이 없습니다
        /// </summary>
        public NullSceneException(string sceneName) : base($"{sceneName} no scene\n{sceneName} 씬이 없습니다") { }
    }

    public class NullScriptMethodException : Exception
    {
        /// <summary>
        /// Failed to execute function because script does not exist
        /// 스크립트가 없어서 함수를 실행하지 못했습니다
        /// </summary>
        public NullScriptMethodException() : base("Failed to execute function because script does not exist\n스크립트가 없어서 함수를 실행하지 못했습니다") { }

        /// <summary>
        /// Failed to execute function because script asdf does not exist
        /// {script} 스크립트가 없어서 함수를 실행하지 못했습니다
        /// </summary>
        public NullScriptMethodException(string scriptName) : base($"Failed to execute function because script {scriptName} does not exist\n{scriptName} 스크립트가 없어서 함수를 실행하지 못했습니다") { }

        /// <summary>
        /// Failed to execute {methodName} function because script {scriptName} does not exist
        /// {script} 스크립트가 없어서 {method} 함수를 실행하지 못했습니다
        /// </summary>
        public NullScriptMethodException(string scriptName, string methodName) : base($"Failed to execute {methodName} function because script {scriptName} does not exist\n{scriptName} 스크립트가 없어서 {methodName} 함수를 실행하지 못했습니다") { }
    }
}