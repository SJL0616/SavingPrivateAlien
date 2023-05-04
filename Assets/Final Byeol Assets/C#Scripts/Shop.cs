using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    public RectTransform uiGroup;
    public Animator anim;

    Player enterPlayer;


    public void Enter(Player player)
    {
        enterPlayer = player;
        uiGroup.anchoredPosition = Vector3.zero;
    }

    public void Exit()
    {
        //인사생략 anim.SetTrigger("doHello");
        uiGroup.anchoredPosition = Vector3.down * 1000;
    }
}
