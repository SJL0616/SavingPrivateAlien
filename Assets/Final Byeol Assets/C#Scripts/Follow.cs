using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    public GameObject target;
    public Vector3 offset;
    public Vector3 offset2;
    public GameObject friend;

    private IEnumerator follow;
    private bool isGameOver = true;

    IEnumerator  following(){
        while(!isGameOver){
        yield return null;
            transform.position = target.transform.position + offset;//카메라의 위치는 현타켓의 움직임을 더한 값이다.
        }
    }
    public void  gameSet(string result){
        if(result == "Win"){
            Debug.Log("WIN");
            target.GetComponent<AlienController>().Victory(target.transform.position);
            GetComponent<CamAudioController>().PlaySound("Victory");
        }else if(result == "WholeWin"){
            GetComponent<CamAudioController>().PlaySound("Victory");
            friend = GameObject.Find("Map").transform.GetChild(1).gameObject;
            target.GetComponent<AlienController>().Victory((friend.transform.position + offset2));
            friend.GetComponentInChildren<AlienFriendController>().anim.SetTrigger("Clear");
            
        }else
        {
            GetComponent<CamAudioController>().PlaySound("Defeat");
        }
        isGameOver = true;
        Invoke("cameraSet",0.5f);
    }
    void cameraSet(){
        transform.position = target.transform.position + new Vector3(3.72f,5.28f,-1.11f);//카메라의 위치는 현타켓의 움직임을 더한 값이다.
    }

    public void CamFollowStart(GameObject player)
    {
        target = player;

        Debug.Log("follwing");
        isGameOver = false;
        follow = following();
        StartCoroutine(follow);
    }

    void Start(){

        
    }

    void Update()
    {
       
    }
}
