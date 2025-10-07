using Cysharp.Threading.Tasks;
using HybridCLR;
using System.Reflection;
using UnityEngine;
using YooAsset;
using static GameFramework.PatchEventDefine;

namespace GameFramework
{
    /// <summary>
    /// 框架启动组件
    /// 功能实现放在GameFramework中 unity工具包装放在GameFrameworkRuntime  游戏具体逻辑放在Game
    /// GameFramework不能反过来引用GameFrameworkRuntime GameFrameworkRuntime不能反过来引用Game 
    /// </summary>
    public sealed class Launch : MonoBehaviour
    {    

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
        private void Awake()
        {
            // 设置游戏帧率。
            Application.targetFrameRate = m_FrameRate;
            // 是否禁止休眠。
            Screen.sleepTimeout = m_NeverSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
            // 设置游戏速度。
            Time.timeScale = m_GameSpeed;
            // 是否后台运行。
            Application.runInBackground = m_RunInBackground;

#if UNITY_5_6_OR_NEWER
            Application.lowMemory += OnLowMemory;
#endif

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

            AssetHandle dllhandle = gamePackage.LoadAssetAsync<TextAsset>("HotUpdate.dll");
            await dllhandle;
            TextAsset dllAsset = (TextAsset)dllhandle.AssetObject;
            byte[] dllBytes = dllAsset.bytes;
            RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, HomologousImageMode.SuperSet);
            Assembly hotUpdateAssembly = Assembly.Load(dllBytes);
            Debug.Log($"热更代码加载完成：{PlayMode}");
            ChangeScence().Forget();
        }

        public async UniTask ChangeScence()
        {
            await UniTask.Delay(3);
            await YooAssets.LoadSceneAsync(ConstStrings.MainHotUpdateScence);
            Debug.Log($"初始化完成！");
        }

        void Update()
        {
            // 检测返回键输入
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("返回键被按下，开始退出应用 ...");
                Application.Quit();
            }
        }

        private void OnApplicationQuit()
        {
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
        }
    }
}
