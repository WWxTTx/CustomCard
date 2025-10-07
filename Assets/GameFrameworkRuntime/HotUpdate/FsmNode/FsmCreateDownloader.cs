using UnityEngine;
using YooAsset;
using Cysharp.Threading.Tasks;
using static GameFramework.Runtime.PatchEventDefine;
namespace GameFramework.Runtime
{
    public class FsmCreateDownloader : IStateNode
    {
        private StateMachine _machine;

        void IStateNode.OnCreate(StateMachine machine)
        {
            _machine = machine;
        }
        void IStateNode.OnEnter()
        {
            EventManager.PublishNow(new PatchStepsChange
            {
                Tips = "创建资源下载器！"
            });
            CreateDownloader();
        }
        void IStateNode.OnUpdate()
        {
        }
        void IStateNode.OnExit()
        {
        }

        void CreateDownloader()
        {
            var packageName = (string)_machine.GetBlackboardValue("PackageName");
            var package = YooAssets.GetPackage(packageName);
            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
            _machine.SetBlackboardValue("Downloader", downloader);

            if (downloader.TotalDownloadCount == 0)
            {
                Debug.Log("Not found any download files !");
                EventManager.PublishNow(new InitializeSucceed
                {
                });
            }
            else
            {
                // 发现新更新文件后，挂起流程系统
                // 注意：开发者需要在下载前检测磁盘空间不足
                EventManager.PublishNow(new FoundUpdateFiles
                {
                    TotalCount = downloader.TotalDownloadCount,
                    TotalSizeBytes = downloader.TotalDownloadBytes
                });
            }
        }
    }
}