using static GameFramework.Runtime.PatchEventDefine;
namespace GameFramework.Runtime
{
    internal class FsmDownloadPackageOver : IStateNode
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
                Tips = "资源文件下载完毕！"
            });
            _machine.ChangeState<FsmClearCacheBundle>();
        }
        void IStateNode.OnUpdate()
        {
        }
        void IStateNode.OnExit()
        {
        }
    }
}