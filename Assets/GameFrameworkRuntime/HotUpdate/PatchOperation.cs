using UnityEngine;
using YooAsset;
using static GameFramework.Runtime.UserEventDefine;

namespace GameFramework.Runtime
{
    public class PatchOperation : GameAsyncOperation
    {
        private enum ESteps
        {
            None,
            Update,
            Done,
        }

        private readonly StateMachine _machine;
        private readonly string _packageName;
        private ESteps _steps = ESteps.None;

        public PatchOperation(string packageName, EPlayMode playMode)
        {
            _packageName = packageName;

            // 注册监听事件
            EventManager.AddListener<UserTryInitialize>(OnHandleEventMessage, this);
            EventManager.AddListener<UserBeginDownloadWebFiles>(OnHandleEventMessage, this);
            EventManager.AddListener<UserTryRequestPackageVersion>(OnHandleEventMessage, this);
            EventManager.AddListener<UserTryUpdatePackageManifest>(OnHandleEventMessage, this);
            EventManager.AddListener<UserTryDownloadWebFiles>(OnHandleEventMessage, this);

            // 创建状态机
            _machine = new StateMachine(this);
            _machine.AddNode<FsmInitializePackage>();
            _machine.AddNode<FsmRequestPackageVersion>();
            _machine.AddNode<FsmUpdatePackageManifest>();
            _machine.AddNode<FsmCreateDownloader>();
            _machine.AddNode<FsmDownloadPackageFiles>();
            _machine.AddNode<FsmDownloadPackageOver>();
            _machine.AddNode<FsmClearCacheBundle>();

            _machine.SetBlackboardValue("PackageName", packageName);
            _machine.SetBlackboardValue("PlayMode", playMode);
        }

        protected override void OnStart()
        {
            _steps = ESteps.Update;
            _machine.Run<FsmInitializePackage>();
        }

        protected override void OnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.Update)
            {
                _machine.Update();
            }
        }
        protected override void OnAbort()
        {
        }

        public void SetFinish()
        {
            _steps = ESteps.Done;
            EventManager.RemoveOwnerListeners(this);
            Status = EOperationStatus.Succeed;
            Debug.Log($"Package {_packageName} patch done !");
        }

        private void OnHandleEventMessage(UserTryInitialize message)
        {
            _machine.ChangeState<FsmInitializePackage>();
        }
        private void OnHandleEventMessage(UserBeginDownloadWebFiles message)
        {
            _machine.ChangeState<FsmDownloadPackageFiles>();
        }
        private void OnHandleEventMessage(UserTryRequestPackageVersion message)
        {
            _machine.ChangeState<FsmRequestPackageVersion>();
        }
        private void OnHandleEventMessage(UserTryUpdatePackageManifest message)
        {
            _machine.ChangeState<FsmUpdatePackageManifest>();
        }
        private void OnHandleEventMessage(UserTryDownloadWebFiles message)
        {
            _machine.ChangeState<FsmCreateDownloader>();
        }
    }
}