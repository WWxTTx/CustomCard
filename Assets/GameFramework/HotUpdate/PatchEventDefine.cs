namespace GameFramework
{
    public class PatchEventDefine
    {
        /// <summary>
        /// 补丁包初始化失败
        /// </summary>
        public struct InitializeFailed : IEventData
        {
        }

        /// <summary>
        /// 补丁流程步骤改变
        /// </summary>
        public struct PatchStepsChange : IEventData
        {
            public string Tips;
        }

        /// <summary>
        /// 发现更新文件
        /// </summary>
        public struct FoundUpdateFiles : IEventData
        {
            public int TotalCount;
            public long TotalSizeBytes;
        }

        /// <summary>
        /// 下载进度更新
        /// </summary>
        public struct DownloadUpdate : IEventData
        {
            public int TotalDownloadCount;
            public int CurrentDownloadCount;
            public long TotalDownloadSizeBytes;
            public long CurrentDownloadSizeBytes;
        }

        /// <summary>
        /// 资源版本请求失败
        /// </summary>
        public struct PackageVersionRequestFailed : IEventData
        {
        }

        /// <summary>
        /// 资源清单更新失败
        /// </summary>
        public struct PackageManifestUpdateFailed : IEventData
        {
        }

        /// <summary>
        /// 网络文件下载失败
        /// </summary>
        public struct WebFileDownloadFailed : IEventData
        {
            public string FileName;
            public string Error;
        }

        public struct InitializeSucceed : IEventData
        {
        }
    }
}