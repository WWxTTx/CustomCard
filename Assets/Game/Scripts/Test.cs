using UnityEngine;
using static GameFramework.Runtime.PlayerInputEventDefine;

namespace GameFramework.Runtime
{
    public class Test : MonoBehaviour
    {
        void Start()
        {
            EventManager.AddListener<CardSelected>(EventHander);
            EventManager.AddListener<CardDrawn>(EventHander);
        }
        private void EventHander(CardDrawn t)
        {
            Debug.Log("玩家抽了一张牌");
        }
        private void EventHander(CardSelected t)
        {
            Debug.Log("玩家选中了一张牌");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                EventManager.PublishNow<CardSelected>(new CardSelected
                {

                });
            }
        }
    }
}