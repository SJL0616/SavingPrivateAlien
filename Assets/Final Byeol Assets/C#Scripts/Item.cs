using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum Type { Ammo, Coin, Grenade, Heart, Weapon};  //아이템의 열거형 타입 선언
    public Type type;
    public int value;

    private void Update()
    {
        transform.Rotate(Vector3.up * 20 * Time.deltaTime); //아이템이 드랍되었을때 회전하는 속도
    }
}
