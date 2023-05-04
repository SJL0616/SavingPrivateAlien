using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/*
게임 매니저 클래스
작성자: 이상준
내용: 게임 시작 / 재시작 / 종료 메서드
마지막 수정일: 2022.6.19
*/
public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    public static int scenesNum = 0;            //씬 넘버 분류용 static 변수
    public static int currentScore = 0;         //현재 점수 저장용 static 변수\
    public static bool isRestart = false;       //재시작인지 판단용 static 변수
    public static bool isStageRe = false;

    public GameObject menuCam;                  // 메뉴 카메라 게임 오브젝트
    public GameObject gameCam;                  // 게임 카메라 게임 오브젝트
    public GameObject player;                   // 플레이어 오브젝트
    public GameObject playerHeathImage;         // 체력 표시 UI

    private SpawnController spawnController;    // 오브젝트 생성 매니저
    public UIManager uIManager;                //스테이지 변경용 매니저
    private DBCtrl db;

    private GameObject[] enemyArr;              //전체 Enemy 오브젝트 배열
    private int enemyCount = 0;
    public int leftEnemy { get { return enemyCount; } set { enemyCount = value; } }

    public bool playerIsDead = false;           //플레이어가 죽었으면 true가 됨
    public bool gameStarted = false;
    public GameObject menuPanel;                //메뉴, 인게임, ESC 용 판넬
    public GameObject gamePanel;
    public Image RunImg;
    public Image DodgeImg;

    public GameObject endPanel;
    public GameObject endRecord;
    public GameObject escPanel;
    public GameObject stagePanel;
    public GameObject titlePanel;
    public GameObject mainCanvas;
    public GameObject backGroundMap;

    public Text maxScoreTxt;                    //누적점수, 현재점수 저장용 Text
    public Text endMaxScoreTxt;
    public Text scorTxt;
    public Text timeTxt;
    private float totalSeconds;

    public Text stageTxt;
    public Text playerHealthTxt;


    // 플레이 타임 UI (미구현)
    // public Text playTimeTxt;
    // public Text playerAmmoTxt;
    // public Text playerCoinTxt;
    // public float playTime;      
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        player = null;
        spawnController = null;
        db = GameObject.FindObjectOfType<DBCtrl>();
    }

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            else
            {
                return instance;
            }
        }
    }



    //씬 전환 함수
    public void LoadStage(int num)
    {
        int currScene = scenesNum;
        scenesNum = num;
        //Debug.Log("to" + "ScPlay" + scenesNum.ToString());
        switch (num)
        {
            case 0:
                SceneManager.UnloadSceneAsync("ScPlay" + currScene.ToString());
                SceneManager.SetActiveScene(SceneManager.GetSceneByName("ScMain"));
                break;
            case 1:
                StartCoroutine(LoadScene("ScPlay1"));
                break;
            case 2:
                StartCoroutine(LoadScene("ScPlay2"));
                break;
        }

        bool gameStarted = num > 0 ? true : false;

        if (menuCam != null)
        {
            menuCam.SetActive(!gameStarted);
        }
        gameCam.SetActive(gameStarted);
        menuPanel.SetActive(!gameStarted);
        gamePanel.SetActive(gameStarted);
        if (menuPanel.activeSelf && !gamePanel.activeSelf)
        {
            ShowStagePanel(false);
            db.ReadUserData("score", ShowScore);
        }

        escPanel.SetActive(false);
        endPanel.SetActive(false);
        backGroundMap.SetActive(!gameStarted);
        if(RunImg.color == Color.gray)
        {
            RunImg.color = new Color(225, 225, 225, 1.0f);
        }

    }

    //씬 전환 코루틴
    IEnumerator LoadScene(string scName)
    {
        yield return null;
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(scName, LoadSceneMode.Additive);

        while (!asyncOperation.isDone)
        {
            Debug.Log("Now Loading : " + asyncOperation.progress * 100 + "%");
            yield return null;
        }

        Scene nThisScene = SceneManager.GetSceneByName(scName);

        if (nThisScene.IsValid())
        {
            Debug.Log("Scene is Valid");

            SceneManager.SetActiveScene(nThisScene);
            //Debug.Log("Scene Name : " + SceneManager.GetActiveScene().name);
            spawnController = GameObject.FindObjectOfType<SpawnController>();
            spawnController.Spawn();

            Camera.main.GetComponent<CamAudioController>().PlaySound("Start");
            gameStarted = true;
            totalSeconds = 0;
            currentScore = 0;
        }
    }

    //다음 스테이지로 이동 버튼 클릭시 실행 메서드
    public void NextStage() {

        SceneManager.UnloadSceneAsync("ScPlay" + scenesNum.ToString());
        LoadStage(scenesNum + 1);
    }

    //게임(스테이지) 재시작버튼 클릭시 실행  메서드
    public void Restart() {
        // isStageRe = true;

        //scenesNum = 1;
        Time.timeScale = 1;
        playerIsDead = false;
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        Debug.Log("NOW SCENE : " + scenesNum);
        LoadStage(scenesNum);
        //SceneManager.LoadScene("Final 1");
    }

    //게임 계속하기 실행(ESC 메뉴에서 계속하기 클릭) 메서드
    public void Continue() {
        Debug.Log("Continue");
        escPanel.SetActive(false);
        Time.timeScale = 1;
    }
    //그만하기 버튼 클릭 시 실행 메서드
    public void QuitStage()
    {
        Time.timeScale = 1;
        LoadStage(0);
    }

    //게임 종료 버튼 클릭 시 실행 메서드
    public void Quit() {
        GameObject.FindObjectOfType<LoginCtrl>().TryGoogleLogout();
        Application.Quit();
    }

    public void OnEscBtnClick()
    {
        Time.timeScale = 0;
        escPanel.SetActive(true);
    }

    public void OnBackButtonClick()
    {
        stagePanel.SetActive(false);
        titlePanel.SetActive(true);
    }

    public void OnRunButtonClick()
    {
        bool isRunning = !player.GetComponentInChildren<AlienController>().isRunning;
        Color imgColor = isRunning ? Color.gray : new Color(225, 225, 225, 1.0f);
        RunImg.color = imgColor;
        player.GetComponentInChildren<AlienController>().isRunning = isRunning;
    }

    public void OnDodgeBtnDown()
    {
        player.GetComponentInChildren<AlienController>().isDodgeClicked = true;
    }
    public void OnDodgeBtnUp()
    {
        player.GetComponentInChildren<AlienController>().isDodgeClicked = false;
    }

    public void OnDodgeButtonClick()
    {
        bool isRunning = !player.GetComponentInChildren<AlienController>().isRunning;
        Color imgColor = isRunning ? Color.gray : new Color(225, 225, 225, 1.0f);
        RunImg.color = imgColor;
        player.GetComponentInChildren<AlienController>().isRunning = isRunning;
    }

    //게임 오브젝트가 죽을 때마다 실행되어서 게임을 클리어할지, 게임 오버를 할지 결정하는 메서드
    public void LifeCalc()
    {
        if (gameStarted)
        {
            if (leftEnemy <= 0)
            {
                GameClear();
            }
            if (playerIsDead)
            {
                GameOver();
            }
        }
        
    }

    //게임 클리어 메서드
    public void GameClear() {
        Debug.Log("win!!!!!!!!!!!!!!!!!!!!!!!");
        gameStarted = false;
        
        //DB에서 현재 기록을 불러와서 현재 기록 시간과 비교하기
        Debug.Log("rawClearTime_" + scenesNum.ToString());
        db.ReadUserData("rawClearTime_" + scenesNum.ToString(), WriteRecord);
        db.ReadUserData("score", AddScore);

        gamePanel.SetActive(false);
        endPanel.transform.GetChild(1).gameObject.SetActive(true);
        endPanel.transform.GetChild(0).gameObject.SetActive(false); // 계속하기 버튼
        
        endPanel.SetActive(true);
        uIManager.ShowStageNum(scenesNum+1);
        db.WriteUserData("clearStage", scenesNum.ToString());

        if(scenesNum > 1){
        gameCam.GetComponent<Follow>().gameSet("WholeWin");
        }else{
        gameCam.GetComponent<Follow>().gameSet("Win");
        }
    }

    //게임 오버 메서드
    public void GameOver(){

        gameStarted = false;
        totalSeconds = 0;
        gamePanel.SetActive(false);
        endPanel.transform.GetChild(1).gameObject.SetActive(false);
        endPanel.transform.GetChild(0).gameObject.SetActive(true); // 다시하기 버튼
        //endMaxScoreTxt.text = string.Format("{0:n0}", yourMaxScore );  
        endPanel.SetActive(true);
        currentScore = 0;
        totalSeconds = 9999;
        db.ReadUserData("rawClearTime_" + scenesNum.ToString(), WriteRecord);
        db.ReadUserData("score", AddScore);

        gameCam.GetComponent<Follow>().gameSet("Lose");
    }

    void AddScore(string myScroe)
    {
        int totalScore = int.Parse(myScroe) + currentScore;
        db.WriteUserData("score", totalScore.ToString());
        ShowScore(myScroe);
    }


    void WriteRecord(string _pastRecord)
    {
        Debug.Log("pastRecord : " + _pastRecord);
        Debug.Log("clearTime compare : " + _pastRecord + " p ? n " + totalSeconds);

        if (_pastRecord != string.Empty && totalSeconds > float.Parse(_pastRecord))
        {
            Debug.Log("기존 기록 유지");
            float pastRecord = float.Parse(_pastRecord);
            endRecord.GetComponent<Text>().text = "최고 기록";
            endRecord.transform.GetChild(1).gameObject.GetComponent<Text>().text = TimeSpan.FromSeconds(pastRecord).ToString("mm\\:ss\\:ff");

        }
        else
        {
            Debug.Log("새기록 갱신");
            //DB에 클리어 타임(Float형, TimeSpan형 저장)
            db.WriteUserData("rawClearTime_" + scenesNum, totalSeconds.ToString());
            db.WriteUserData("ClearTime_" + scenesNum, TimeSpan.FromSeconds(totalSeconds).ToString("mm\\:ss\\:ff"));
            // 기록 갱신이 된 경우
            endRecord.GetComponent<Text>().text = "최고 기록 갱신!";
            endRecord.GetComponent<Text>().color = Color.yellow;
            endRecord.transform.GetChild(1).gameObject.GetComponent<Text>().text = TimeSpan.FromSeconds(totalSeconds).ToString("mm\\:ss\\:ff");
            endRecord.transform.GetChild(1).gameObject.GetComponent<Text>().color = Color.yellow;
        }
        
       
        // 기록 갱신이 안됬을 경우
        totalSeconds = 0;
    }


    public void ShowStagePanel(bool showPanel = true)
    {
        stagePanel.SetActive(showPanel);
        titlePanel.SetActive(!showPanel);
    }

    public void ShowScore(string totalScore)
    {
        Debug.Log("ShowScore");
       
        int text = int.Parse(totalScore) + currentScore;
        string addText = string.Empty;
        if (currentScore != 0)
        {
            addText = "(+" + currentScore + ")";
        }
        Debug.Log(addText);
        maxScoreTxt.text = string.Format("{0:n0}", text);
        endMaxScoreTxt.text= string.Format("{0:n0}", text) + addText;
        currentScore = 0;
    }
    

    void LateUpdate()
    {

        stageTxt.text = "STAGE" + scenesNum;
        scorTxt.text = string.Format("{0:n0}", currentScore);
        if (gameStarted)
        {
            totalSeconds += Time.deltaTime;
            
            timeTxt.text = TimeSpan.FromSeconds(totalSeconds).ToString("mm\\:ss\\:ff");
            //TimeSpan time = TimeSpan.FromSeconds(totalSeconds);
            //timeTxt.text = string.Format("{0:00}:{1:00}:{2:0}", time.Minutes, time.Seconds, time.Milliseconds);

        }

        //좌측 상단 체력바 설정
        if (player != null)
        {
           // playerHealthTxt.text = player.GetComponentInChildren<AlienController>().HP + " / 100";
            int currentInt = 100 - player.GetComponentInChildren<AlienController>().HP;
            playerHeathImage.transform.localScale = new Vector3(1 - (currentInt * 0.01f), 1, 1);
        }
        

        // ESC 누를시 메뉴 UI표시, 시간 정지
        if(Input.GetKeyDown(KeyCode.Escape)){
          Time.timeScale = 0;
          escPanel.SetActive(true);
        }

    }
}
