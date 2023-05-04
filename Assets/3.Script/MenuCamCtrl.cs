using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCamCtrl : MonoBehaviour
{
    private Animator anim;
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        Debug.Log("Start Rotate");
        anim.SetFloat("Rotate",1f);
    }

    private void OnDisable()
    {
        Debug.Log("Stop Rotate");
        anim.SetFloat("Rotate", 0f);
    }
}
