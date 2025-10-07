using GameFramework.Runtime;
using UnityEngine;
using static GameFramework.Runtime.PlayerInputEventDefine;

public class Test : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
            EventManager.PublishNow(new CardSelected
            {

            });
        }
    }
}
