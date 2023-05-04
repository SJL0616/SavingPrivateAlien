using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPoolController : MonoBehaviour
{

    public GameObject[] bulletPrefabs;   //�Ѿ� �������� �ν����Ϳ��� �ִ� �迭.
    public int poolCount;             //Pool���� ������ �Ѿ� ����
    [SerializeField]
    private List<GameObject> BulletPool1; //�ν����� Ȯ�ο� �Ѿ� �迭
    //�Ѿ� ������Ʈ�� �̸��� ����ؼ� �ش� ������Ʈ�� �ִ� �迭�� �ҷ���.
    private Dictionary<string, List<GameObject>> poolMap = new Dictionary<string, List<GameObject>>();

    private void Awake()
    {
        // �Ѿ� Pool�� ���� �����ϵ��� �Ѿ� ���� Map ���� (Key : �Ѿ��̸� ,Value : �Ѿ� Pool(List��))
        for (int i = 0; i < bulletPrefabs.Length; i++)
        {
            BulletPool1 = new List<GameObject>(); 
            poolMap.Add(bulletPrefabs[i].name, BulletPool1);

        }
    }

    //�Ѿ��� ���� Pool�� �����ϴ� �޼���
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

    //�Ѿ� ������Ʈ ���� �� ��ȯ�ϴ� �޼���
    private GameObject CreateBullet(GameObject bullet)
    {
        GameObject oneBullet = Instantiate(bullet) as GameObject;
        oneBullet.transform.SetParent(transform);
        oneBullet.GetComponent<ProjectileMover>().bulletPool = this;
        oneBullet.SetActive(false);
        return oneBullet;
    }

    //�Ѿ� ������Ʈ�� �迭���� �ִ� �޼���
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

    //�Ѿ� ������Ʈ�� �迭���� ���� �޼���
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

            //�뷮 �̻��� �Ѿ� ��� ���� �߰��ؾ���
        }
        return bullet;
    }

}
