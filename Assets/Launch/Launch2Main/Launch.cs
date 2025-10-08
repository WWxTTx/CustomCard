using Cysharp.Threading.Tasks;
using HybridCLR;
using Obfuz;
using Obfuz.EncryptionVM;
using System.Reflection;
using UnityEngine;
using YooAsset;
using static GameFramework.PatchEventDefine;

namespace GameFramework
{
    /// <summary>
    /// 框架启动组件
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
        /// 备注：泛型类 不要 在主包中创建，热更包中新增
        /// 需要使用Burst编译器的代码必须在主包中 这类情况可以接受泛型补充
        /// </summary>
        private async void Start()
        {
            // 初始化资源系统
            YooAssets.Initialize();

            // 开始更新流程
            var operation = new PatchOperation("DefaultPackage", PlayMode);
            YooAssets.StartOperation(operation);
            EventManager.AddListener<InitializeSucceed>(OnInitializeSucceed);
            await operation;
        }

        /// <summary>
        /// 下载清理完成 或没有资源需要更新
        /// </summary>
        /// <param name="completed"></param>
        public void OnInitializeSucceed(InitializeSucceed completed)
        {
            LoadDll().Forget();
        }

        /// <summary>
        /// 泛型补充 热更代码加载
        /// </summary>
        /// <returns></returns>
        public async UniTask LoadDll()
        {
            // 设置默认的资源包
            var gamePackage = YooAssets.GetPackage("DefaultPackage");
            YooAssets.SetDefaultPackage(gamePackage);

#if !UNITY_EDITOR
            //获取密码
            AssetHandle keyhandle = gamePackage.LoadAssetAsync<TextAsset>("Obfuz");
            await keyhandle;
            TextAsset key = (TextAsset)keyhandle.AssetObject;
            EncryptionService<DefaultStaticEncryptionScope>.Encryptor = new GeneratedEncryptionVirtualMachine(key.bytes);

            //获取热更程序集 加载
            AssetHandle dllhandle = gamePackage.LoadAssetAsync<TextAsset>("HotUpdate.dll");
            await dllhandle;
            TextAsset dllAsset = (TextAsset)dllhandle.AssetObject;
            RuntimeApi.LoadMetadataForAOTAssembly(dllAsset.bytes, HomologousImageMode.SuperSet);
            Assembly hotUpdateAssembly = Assembly.Load(dllAsset.bytes);
            Debug.Log($"热更代码加载完成：{PlayMode}");
#endif

            await UniTask.Delay(1000);
            await YooAssets.LoadSceneAsync(ConstStrings.MainHotUpdateScence);
            Debug.Log($"初始化完成！");
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
