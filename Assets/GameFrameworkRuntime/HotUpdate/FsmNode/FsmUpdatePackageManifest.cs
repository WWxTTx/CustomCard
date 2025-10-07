using Cysharp.Threading.Tasks;
using YooAsset;
using static GameFramework.Runtime.PatchEventDefine;
namespace GameFramework.Runtime
{
    public class FsmUpdatePackageManifest : IStateNode
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
                Tips = "更新资源清单！"
            });
            UpdateManifest().Forget();
        }
        void IStateNode.OnUpdate()
        {
        }
        void IStateNode.OnExit()
        {
        }

        private async UniTask UpdateManifest()
        {
            var packageName = (string)_machine.GetBlackboardValue("PackageName");
            var packageVersion = (string)_machine.GetBlackboardValue("PackageVersion");
            var package = YooAssets.GetPackage(packageName);
            var operation = package.UpdatePackageManifestAsync(packageVersion);
            await operation;

            if (operation.Status != EOperationStatus.Succeed)
            {
                EventManager.PublishNow(new PackageManifestUpdateFailed
                {
                });
            }
            else
            {
                _machine.ChangeState<FsmCreateDownloader>();
            }
        }
    }
}