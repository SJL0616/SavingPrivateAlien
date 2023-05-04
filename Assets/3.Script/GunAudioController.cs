using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
�÷��̾� �� �߻� ȿ���� ��Ʈ�ѷ�
�ۼ���: �̻���
����:�� �߻� ȿ���� ��� - ������
������ ������: 2022.6.19
*/
public class GunAudioController : MonoBehaviour
{   
    public AudioClip audioShot;     // ���� ȿ���� Ŭ��
    public AudioClip audioReload;   // ������ ȿ���� Ŭ��
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
