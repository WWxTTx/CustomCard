namespace GameFramework.Runtime
{
    public class UserEventDefine
    {
        /// <summary>
        /// 用户尝试再次初始化资源包
        /// </summary>
        public struct UserTryInitialize : IEventData
        {
        }

        /// <summary>
        /// 用户开始下载网络文件
        /// </summary>
        public struct UserBeginDownloadWebFiles : IEventData
        {
        }

        /// <summary>
        /// 用户尝试再次请求资源版本
        /// </summary>
        public struct UserTryRequestPackageVersion : IEventData
        {
        }

        /// <summary>
        /// 用户尝试再次更新补丁清单
        /// </summary>
        public struct UserTryUpdatePackageManifest : IEventData
        {
        }

        /// <summary>
        /// 用户尝试再次下载网络文件
        /// </summary>
        public struct UserTryDownloadWebFiles : IEventData
        { 
        }
    }
}