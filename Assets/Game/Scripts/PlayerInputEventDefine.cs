namespace GameFramework.Runtime
{
    /// <summary>
    /// 玩家输入事件定义
    /// </summary>
    public class PlayerInputEventDefine
    {
        /// <summary>
        /// 选择卡牌
        /// </summary>
        public struct CardSelected : IEventData
        {
            public int CardIndex;
        }

        /// <summary>
        /// 使用卡牌
        /// </summary>
        public struct CardUsed : IEventData
        {
            public int CardIndex;
        }

        /// <summary>
        /// 抽牌
        /// </summary>
        public struct CardDrawn : IEventData
        {
        }
    }
}