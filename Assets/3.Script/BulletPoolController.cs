using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPoolController : MonoBehaviour
{

    public GameObject[] bulletPrefabs;   //총알 프리펍을 인스펙터에서 넣는 배열.
    public int poolCount;             //Pool에서 생성할 총알 갯수
    [SerializeField]
    private List<GameObject> BulletPool1; //인스펙터 확인용 총알 배열
    //총알 오브젝트의 이름을 사용해서 해당 오브젝트가 있는 배열을 불러옴.
    private Dictionary<string, List<GameObject>> poolMap = new Dictionary<string, List<GameObject>>();

    private void Awake()
    {
        // 총알 Pool을 쉽게 제어하도록 총알 별로 Map 저장 (Key : 총알이름 ,Value : 총알 Pool(List형))
        for (int i = 0; i < bulletPrefabs.Length; i++)
        {
            BulletPool1 = new List<GameObject>(); 
            poolMap.Add(bulletPrefabs[i].name, BulletPool1);

        }
    }

    //총알을 만들어서 Pool에 저장하는 메서드
    public void Initialize(string name = "")
    {
        GameObject bulletPrefab = null;

        if (string.IsNullOrEmpty(name))
        {
            bulletPrefab = bulletPrefabs[0];
        }
        else
        {
            for(int i = 1; i < bulletPrefabs.Length; i++)
            {
                if (bulletPrefabs[i].name == name)
                {
                    bulletPrefab = bulletPrefabs[i];
                }
            }
        }

        if (poolMap.ContainsKey(bulletPrefab.name))
        {
            List<GameObject> bulletPool = poolMap[bulletPrefab.name];
            for (int i = 0; i < poolCount; i++)
            {
                bulletPool.Add(CreateBullet(bulletPrefab));
            }
        }
    }

    //총알 오브젝트 생성 후 반환하는 메서드
    private GameObject CreateBullet(GameObject bullet)
    {
        GameObject oneBullet = Instantiate(bullet) as GameObject;
        oneBullet.transform.SetParent(transform);
        oneBullet.GetComponent<ProjectileMover>().bulletPool = this;
        oneBullet.SetActive(false);
        return oneBullet;
    }

    //총알 오브젝트를 배열에서 넣는 메서드
    public void PushToPool(string name, GameObject bullet)
    {
        List<GameObject> bulletPool = null;
        if (poolMap.ContainsKey(name))
        {
            bulletPool = poolMap[name];
            if (bulletPool.Count >= poolCount) return;
            bullet.transform.SetParent(transform);
            bullet.SetActive(false);
            bulletPool.Add(bullet);
        }
    }

    //총알 오브젝트를 배열에서 빼는 메서드
    public GameObject PopFromPool(string name)
    {
        GameObject bullet = null;
        List<GameObject> bulletPool = null;
        if (poolMap.ContainsKey(name))
        {
            bulletPool = poolMap[name];
            bullet = bulletPool[0];
            bullet.transform.SetParent(null);
            bulletPool.RemoveAt(0);

            //용량 이상의 총알 사용 로직 추가해야함
        }
        return bullet;
    }

}
