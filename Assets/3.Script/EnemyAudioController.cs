using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
�� ȿ���� ��Ʈ�ѷ�
�ۼ���: �̻���
����:�� �߻� - ������, ����  / �״� �Ҹ�
������ ������: 2022.6.19
*/
public class EnemyAudioController : MonoBehaviour
{
    public AudioClip audioM5;            // M5 ���� Ŭ��
    public AudioClip audioShotgun;            // ���� ���� Ŭ��
    public AudioClip audioShotgunReload;      // ���� ������ Ŭ��
    public AudioClip audioDead;            // �״� �Ҹ� Ŭ��
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    //ȿ���� ��� �޼���.
    public void PlaySound(string name){
        switch(name){// string ���� ���� ȿ���� ���
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
