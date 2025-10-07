using Cysharp.Threading.Tasks;
using YooAsset;
using static GameFramework.Runtime.PatchEventDefine;
namespace GameFramework.Runtime
{
    public class FsmDownloadPackageFiles : IStateNode
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
                Tips = "开始下载资源文件！"
            });
            BeginDownload().Forget();
        }

        void IStateNode.OnUpdate()
        {
        }
        void IStateNode.OnExit()
        {
        }

        private async UniTask BeginDownload()
        {
            var downloader = (ResourceDownloaderOperation)_machine.GetBlackboardValue("Downloader");
            downloader.DownloadErrorCallback = DownloadErrorCallback;
            downloader.DownloadUpdateCallback = DownloadUpdateCallback;
            downloader.BeginDownload();
            await downloader;

            // 下载结果
            if (downloader.Status == EOperationStatus.Succeed)
                _machine.ChangeState<FsmDownloadPackageOver>();
        }

        // 下载过程
        private void DownloadUpdateCallback(DownloadUpdateData data)
        {
            EventManager.PublishNow(new DownloadUpdate
            {
                TotalDownloadCount = data.TotalDownloadCount,
                CurrentDownloadCount = data.CurrentDownloadCount,
                TotalDownloadSizeBytes = data.TotalDownloadBytes,
                CurrentDownloadSizeBytes = data.CurrentDownloadBytes
            });
        }

        // 下载失败
        private void DownloadErrorCallback(DownloadErrorData data)
        {
            EventManager.PublishNow(new WebFileDownloadFailed
            {
                FileName = data.FileName,
                Error = data.ErrorInfo
            });
        }
    }
}