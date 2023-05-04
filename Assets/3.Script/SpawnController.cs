using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
    public GameObject playerObj;
    public GameObject player;
    public GameObject enemyM5;
    public GameObject enemysShotGun;

    public GameObject playerSpawnPoint;         // 플레이어 생성 지점
    public GameObject[] enemySpawnPoints;      // 적 스폰 포인트 배열

    private int stageNum;


    private void Awake()
    {
        enemySpawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoints");
        playerSpawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");
        stageNum = GameManager.scenesNum;
    }

    public void Spawn()
    {
        PlayerSpawn();
        // 적 오브젝트 생성
        //스테이지 별로 나오는 적 종류가 다름
        EnemySpawn(stageNum);
    }

    void PlayerSpawn()
    {
        if (player == null)
        {
            player = Instantiate(playerObj) as GameObject;
            GameObject.FindObjectOfType<GameManager>().player = player;
        }

        //플레이어 오브젝트 생성
        player.gameObject.SetActive(true);
        player.transform.position = playerSpawnPoint.transform.position;
    }

    void EnemySpawn(int stageNum)
    {
        int kindLimit = 0;
        int enemyKinds = 0;
        switch (stageNum)
        {
            case 1:
            case 2:
                kindLimit = 1;
                break;
        }

        for (int i = 0; i < enemySpawnPoints.Length; i++)
        {
            if (enemyKinds > kindLimit) enemyKinds = 0;
            Transform spawnPoint = enemySpawnPoints[i].transform.GetChild(0);
            GameObject enemy = null;
            switch (enemyKinds)
            {
                case 0:
                    enemy = Instantiate(enemyM5, spawnPoint.position, Quaternion.identity) as GameObject;
                    enemy.GetComponent<EnemyMovementContorller>().SetWayPoints(enemySpawnPoints[i]);
                    break;
                case 1:
                    enemy = Instantiate(enemysShotGun, spawnPoint.position, Quaternion.identity) as GameObject;
                    enemy.GetComponent<EnemyMovementContorller>().SetWayPoints(enemySpawnPoints[i]);
                    break;
            }

            enemyKinds++;

        }
         GameManager.Instance.leftEnemy = enemySpawnPoints.Length;
        //if (GameManager.scenesNum == 1)
        //{
        //    GameManager.Instance.leftEnemy = 0;
        //}
        //else
        //{
        //    GameManager.Instance.leftEnemy = enemySpawnPoints.Length;
        //}
        //    GameManager.Instance.leftEnemy = 0;
        //Invoke("ClearTest", 2.7f);
        //GameManager.Instance.playerIsDead = true;
        Debug.Log("Left enemy : " + GameManager.Instance.leftEnemy);
    }

    void ClearTest()
    {
        GameManager.Instance.LifeCalc();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
