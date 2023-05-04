using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private GameManager gameManager;
    private DBCtrl db;
    private void Awake()
    {
        gameManager = GameObject.FindObjectOfType<GameManager>();
        db = GameObject.FindObjectOfType<DBCtrl>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // �������� ��ư �ʱ�ȭ, �̹��� �ʱ�ȭ
    public void InitStageNum(string _clearStage)
    {
        int clearStage = int.Parse(_clearStage);
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject btn = transform.GetChild(i).gameObject;
            int stageIndex = i + 1;
            btn.GetComponentInChildren<Text>().text = stageIndex.ToString();
            
            btn.GetComponent<Button>().onClick.AddListener(() => StartStage(stageIndex));
            if(stageIndex <= clearStage+1)
            {//1�������� ���� Xǥ��
                Image XImg = btn.transform.GetChild(1).GetComponent<Image>();
                XImg.color= new Color(0, 0, 0, 0);
            }
        }
    }

    // �������� ��ư �����ִ� Xǥ�ÿ� �̹��� color���� ����ȭ �Լ�
    public void ShowStageNum(int sceneNum)
    {
        GameObject btn = transform.GetChild(sceneNum-1).gameObject;
        string num = btn.GetComponentInChildren<Text>().text;
        if (sceneNum == int.Parse(num))
        {
            Image XImg = btn.transform.GetChild(1).GetComponent<Image>();
            XImg.color = new Color(0, 0, 0, 0);
        }
    }


    //�������� ��ư Ŭ���� ȣ���Լ� 
    //=> �ش� ��ȣ�� �������� �ε��Լ��� ȣ��.
    public void StartStage(int num)
    {
        GameObject btn = transform.GetChild(num - 1).gameObject;
        Image XImg = btn.transform.GetChild(1).GetComponent<Image>();
        Color  whiteColor = new Color(0, 0, 0, 0);
        if (XImg.color != whiteColor) return;

        gameManager.LoadStage(num);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
