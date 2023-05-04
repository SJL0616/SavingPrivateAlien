using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienFriendController : MonoBehaviour
{
    public  Animator anim;
    // Start is called before the first frame update
     public void VictoryFriend(){
        Debug.Log("isClear");
        anim.SetTrigger("Clear");
    }

    void Awake(){
        anim = GetComponent<Animator>();
        
    }
    void Start(){
        anim.SetTrigger("Scared");
    }
   
    // Update is called once per frame
    void Update()
    {
        
    }
}
