using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 적 총 발사 컨트롤러
작성자: 이상준
내용:총 발사 - 라이플, 샷건 
마지막 수정일: 2022.6.19
*/
public class EnemyGunController : MonoBehaviour
{
    public GameObject gun;             // 총 게임오브젝트
    public GameObject holder;             // 총 발사 위치 게임오브젝트
    //public GameObject bullet;             // 총알 게임오브젝트
    private BulletPoolController bulletPool;
    public GameObject sparkPrefab;             // 총 발포 스파크 파티클
    private GameObject spark;             // 총 발포 스파크 파티클
    private int fireRate = 7;             
    public GameObject shotgunRange;       // 샷건 범위
    private BoxCollider collider;         // 샷건용 박스 콜라이더   
    private new EnemyAudioController audio; 
    private EnemyMovementContorller move;
    private Animator anim;
    private IEnumerator shootIEnumerator;


    private void Awake()
    {
        shootIEnumerator = null;
        audio = GetComponent<EnemyAudioController>();
        move = GetComponent<EnemyMovementContorller>();
        anim = GetComponent<Animator>();
        spark = null;
        if (shotgunRange != null)
        {
            collider = shotgunRange.GetComponent<BoxCollider>();
            collider.enabled = false;
        }
        bulletPool = transform.GetComponentInChildren<BulletPoolController>();
    }

    void Start()
    {
        bulletPool.Initialize();
    }

    //사격 모드 제어 메서드
    public void Shooting(string type, bool isShooting){
        if(isShooting){ // 사격 모드일 때
           
            switch(type){ // 총 종류에 따라 다른 총 발사
                case "M5": // 라이플 (M5)
                    if (shootIEnumerator != null) return;
                    shootIEnumerator = M5ShotGun(type, 0.5f);
                    StartCoroutine(shootIEnumerator);
                    break;
                case "Shotgun":
                    if (fireRate % 7 == 0)
                    {   move.shoot(true);
                        StartCoroutine(PlayShotGunSound());
                        StartCoroutine(shootShotGun());
                    }
                    fireRate++;
                    break;
            }
        }
        else
        {
            if(shootIEnumerator != null)
            {
                StopCoroutine(shootIEnumerator);
                shootIEnumerator = null;
            }
           
        }
    }

    IEnumerator M5ShotGun(string type ,float delay)
    {
        yield return null;
        while (true)
        {
            yield return new WaitForSeconds(delay);
            move.shoot(true); //총 발사 애니메이션 시작
            audio.PlaySound("M5"); //총 발사 음향 재생
            holder.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles);
            GameObject bullets = bulletPool.PopFromPool(type);
            bullets.transform.position = holder.transform.position;
            bullets.transform.rotation = holder.transform.rotation;
            bullets.SetActive(true);
            bullets.GetComponent<ProjectileMover>().shot();

            Rigidbody rigid = bullets.GetComponent<Rigidbody>();
            rigid.AddForce(transform.forward * 50.0f, ForceMode.Impulse);
        }
    }

    // 샷건 사격 제어 메서드
    void ShootM5(){
       anim.SetBool("Shoot_b",false);
    }
    IEnumerator shootShotGun(){ // 샷건 사격을 실행시키는 코루틴 메서드
        gun.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles);
        holder.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles);
        if (spark == null) spark = Instantiate(sparkPrefab, holder.transform.position, holder.transform.rotation) as GameObject;
        //샷건 발포 시 스파크 파티클 플레이
        spark.transform.position = holder.transform.position;
        spark.transform.rotation = holder.transform.rotation;
        spark.SetActive(true);
        var flashPs = spark.GetComponent<ParticleSystem>();
        if(flashPs != null){
            collider.enabled = true;
            flashPs.Play();
            Debug.Log(flashPs.main.duration);
            yield return new WaitForSeconds(flashPs.main.duration);
            spark.SetActive(false);
            yield return new WaitForSeconds(0.5f - flashPs.main.duration);
            collider.enabled = false;
        }
    }


    IEnumerator PlayShotGunSound()
    {
        audio.PlaySound("Shotgun");
        AudioSource audioSource = audio.GetComponent<AudioSource>();
        yield return new WaitForSeconds(0.9f);
        if (!audioSource.isPlaying)
        {
            audio.PlaySound("ShotgunReload");
            anim.SetBool("Shoot_b",false);
        }
    }
   

    // Update is called once per frame
    void Update()
    {
        
    }
}
