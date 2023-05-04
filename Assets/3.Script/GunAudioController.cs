using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
플레이어 총 발사 효과음 컨트롤러
작성자: 이상준
내용:총 발사 효과음 재생 - 라이플
마지막 수정일: 2022.6.19
*/
public class GunAudioController : MonoBehaviour
{   
    public AudioClip audioShot;     // 발포 효과음 클립
    public AudioClip audioReload;   // 재장전 효과음 클립
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayGunSound(string action){
        switch(action){
            case "Shot":
                audioSource.clip = audioShot;
                audioSource.volume = 0.1f;
                break;
            case "Reload":
                audioSource.clip = audioReload;
                audioSource.volume = 0.4f;
                break;
        }
        audioSource.Play();
    }
   

}
