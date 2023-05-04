using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
적 효과음 컨트롤러
작성자: 이상준
내용:총 발사 - 라이플, 샷건  / 죽는 소리
마지막 수정일: 2022.6.19
*/
public class EnemyAudioController : MonoBehaviour
{
    public AudioClip audioM5;            // M5 발포 클립
    public AudioClip audioShotgun;            // 샷건 발포 클립
    public AudioClip audioShotgunReload;      // 샷건 재장전 클립
    public AudioClip audioDead;            // 죽는 소리 클립
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    //효과음 재생 메서드.
    public void PlaySound(string name){
        switch(name){// string 값에 따른 효과음 재생
            case "M5":
                audioSource.clip = audioM5;
                audioSource.volume = 0.05f;             
            break;
            case "Shotgun":
                audioSource.clip = audioShotgun;
                audioSource.volume = 0.2f;
            break;
            case "ShotgunReload":
                audioSource.clip = audioShotgunReload;
                audioSource.volume = 0.2f;
            break;
            case "Dead":
                audioSource.clip = audioDead;
                audioSource.volume = 0.1f;             
            break;
        }
         audioSource.Play();
    }
}
