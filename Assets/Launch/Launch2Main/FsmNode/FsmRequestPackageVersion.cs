using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;
using static GameFramework.PatchEventDefine;
namespace GameFramework
{
    internal class FsmRequestPackageVersion : IStateNode
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
                Tips = "请求资源版本！"
            });
            UpdatePackageVersion();
        }
        void IStateNode.OnUpdate()
        {
        }
        void IStateNode.OnExit()
        {
        }

        private async void UpdatePackageVersion()
        {
            var packageName = (string)_machine.GetBlackboardValue("PackageName");
            var package = YooAssets.GetPackage(packageName);
            var operation = package.RequestPackageVersionAsync();
            await operation;

            if (operation.Status != EOperationStatus.Succeed)
            {
                EventManager.PublishNow(new PackageVersionRequestFailed
                {
                });
                Debug.LogWarning(operation.Error);
            }
            else
            {
                Debug.Log($"Request package version : {operation.PackageVersion}");
                _machine.SetBlackboardValue("PackageVersion", operation.PackageVersion);
                _machine.ChangeState<FsmUpdatePackageManifest>();
            }
        }
    }
}