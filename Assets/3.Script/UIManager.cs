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

    // 스테이지 버튼 초기화, 이미지 초기화
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
            {//1스테이지 빼고 X표시
                Image XImg = btn.transform.GetChild(1).GetComponent<Image>();
                XImg.color= new Color(0, 0, 0, 0);
            }
        }
    }

    // 스테이지 버튼 위에있는 X표시용 이미지 color값을 투명화 함수
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


    //스테이지 버튼 클릭시 호출함수 
    //=> 해당 번호의 스테이지 로드함수를 호출.
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
