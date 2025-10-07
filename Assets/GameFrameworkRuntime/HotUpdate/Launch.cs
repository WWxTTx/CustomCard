using Cysharp.Threading.Tasks;
using HybridCLR;
using UnityEngine;
using YooAsset;
using static GameFramework.Runtime.PatchEventDefine;

namespace GameFramework.Runtime
{
    /// <summary>
    /// 框架启动组件
    /// 功能实现放在GameFramework中 unity工具包装放在GameFrameworkRuntime  游戏具体逻辑放在Game
    /// GameFramework不能反过来引用GameFrameworkRuntime GameFrameworkRuntime不能反过来引用Game 
    /// </summary>
    public sealed class Launch : GameFrameworkComponent
    {    
        public static Launch Instance { get; private set; }

        /// <summary>
        /// 资源系统运行模式
        /// </summary>
        public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;

        private float m_GameSpeed = 1f;
        private int m_FrameRate = 60;
        private bool m_RunInBackground = true;
        private bool m_NeverSleep = true;


        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            Instance = this;

            // 设置游戏帧率。
            Application.targetFrameRate = m_FrameRate;
            // 是否禁止休眠。
            Screen.sleepTimeout = m_NeverSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
            // 设置游戏速度。
            Time.timeScale = m_GameSpeed;
            // 是否后台运行。
            Application.runInBackground = m_RunInBackground;

            // 工具类 待拓展
            Utility.Json.SetJsonHelper(new DefaultJsonHelper());
            Utility.Text.SetTextHelper(new DefaultTextHelper());
            Utility.Compression.SetCompressionHelper(new DefaultCompressionHelper());

#if UNITY_5_6_OR_NEWER
            Application.lowMemory += OnLowMemory;
#endif

            Debug.Log($"Game Framework Version: {Version.GameFrameworkVersion}");
            Debug.Log($"Game Version: {Version.GameVersion} ({Version.InternalGameVersion})");
            Debug.Log($"Unity Version: {Application.unityVersion}");
            Debug.Log($"屏幕像素密度: {Screen.dpi}");
            Debug.Log($"资源系统运行模式：{PlayMode}");
        }

        /// <summary>
        /// 备注：泛型类必须在主包中
        /// 需要使用Burst编译器的代码必须在主包中
        /// 场景启动 yoo自动加载依赖资源 Unity通过vtable调用生命周期 避免混淆改变了类名 反射无法访问
        /// </summary>
        private async void Start()
        {
            // 初始化资源系统
            YooAssets.Initialize();

            // 加载更新页面
            var go = Resources.Load<GameObject>("PatchWindow");
            Instantiate(go);

            // 开始更新流程
            var operation = new PatchOperation("DefaultPackage", PlayMode);
            YooAssets.StartOperation(operation);
            EventManager.AddListener<InitializeSucceed>(OnInitializeSucceed);
            await operation;
        }

        private void Update()
        {
            GameFrameworkEntry.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        /// <summary>
        /// 下载清理完成 或没有资源需要更新
        /// 泛型补充 热更代码加载
        /// </summary>
        /// <param name="completed"></param>
        public async void OnInitializeSucceed(InitializeSucceed completed)
        {
            // 设置默认的资源包
            var gamePackage = YooAssets.GetPackage("DefaultPackage");
            YooAssets.SetDefaultPackage(gamePackage);

            await YooAssets.LoadSceneAsync(ConstStrings.MainHotUpdateScence);

            Debug.Log($"初始化完成！");
        }

        private void OnDestroy()
        {
            GameFrameworkEntry.Shutdown();
        }

        private void OnApplicationQuit()
        {
            GameFrameworkEntry.Shutdown();
#if UNITY_5_6_OR_NEWER
            Application.lowMemory -= OnLowMemory;
#endif
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        private void OnLowMemory()
        {
            Debug.Log("检测到设备内存较低，开始释放对象池资源 ...");

            ObjectPoolComponent objectPoolComponent = GameEntry.GetComponent<ObjectPoolComponent>();
            if (objectPoolComponent != null)
            {
                objectPoolComponent.ReleaseAllUnused();
            }
        }
    }
}
