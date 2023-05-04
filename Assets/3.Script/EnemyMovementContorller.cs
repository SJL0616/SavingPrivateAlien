using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/*
 적 이동 컨트롤러
작성자: 이상준
내용:적 이동(네비메시 이용), 총 발사, 구르기 , 에이밍에 따른 회전, 애니메이션
마지막 수정일: 2022.6.22
*/
public class EnemyMovementContorller : MonoBehaviour
{

    private Animator animator;
    private NavMeshAgent agent;
    private int HP = 100;

    public Transform[] wayPoints;            //정찰 지점 저장용 Transform 배열
    public GameObject wayPointsGroup;        //정찰 지점 모음 게임오브젝트
    public int nextIdx = 1;                  
    public float speed = 1.9f;               
    public float damping = 5.0f;             
    private Transform tr;

    private float speed_f;
    public GameObject gun;
    private IEnumerator coroutine;
    private Rigidbody rigid;
    public bool isBouncing = false;
    private bool isPatrolling = false;
    public bool isInvincible = false;
    public bool isDetouring = false;
    private SkinnedMeshRenderer sr;
    private Color normalColor;
    private Color damagedColor = new Color(181/255f,76/255f,76/255f);
    public Camera cam;


    //충돌 처리 로직
    //내용: 플레이어 무기에 맞을 시 피격 로직
    void OnCollisionEnter(Collision other){
        if(other.gameObject.tag == "AlienWeapon" && !isInvincible){ // 플레이어의 총알에 맞을 시
            if(isPatrolling){
                StopCoroutine(coroutine);   // 정찰 모드 비활성화
                GetComponent<EnemyDetectionController>().IntoCombatMode();// 교전 모드 활성화
                isPatrolling = false;                 
            }
            onDamaged(other.gameObject.transform.position);//물리적으로 살짝 뜨는 효과(피격) 주는 메서드 실행
        }
    }
    //적 피격 함수 
    //내용 : 데미지 입는 로직, 물리적으로 살짝 뜨는 효과, 순간적으로 컬러 빨간색으로 표시
    void onDamaged(Vector3 target){
        HP -= 30;
        sr.material.color = damagedColor;
        Invoke("setColor",0.3f);
        if(!isBouncing){
            agent.enabled = false; //NavMeshAgent를 잠깐 off
            Vector3 pos = (transform.position - target);
            
            StartCoroutine(startBounce(target));
        }
        if(HP <= 0 ){
            isInvincible = true;
            animator.SetBool("Death_b",true);
            int type = Random.Range(1,2);
            animator.SetInteger("DeathType_int",type);
            GetComponent<EnemyDetectionController>().isDead();
            walk(false);
            shoot(false);
            animator.SetInteger("WeaponType_int",10);
        }
    }
    void setColor(){
        sr.material.color = normalColor;
    }
    
    IEnumerator startBounce(Vector3 pos){ // 패트롤을 실행시키는 코루틴 메서드
        isBouncing = true;
        rigid.AddExplosionForce(20f,pos,100f,5f); //띄운 뒤
        
        yield return new WaitForSeconds(1.9f);

        agent.enabled = true;
        isBouncing = false;
        // 3초 뒤 NavMeshAgent를 다시 on
    }

    //걷기 애니메이션 제어 메서드
    public void walk(bool inputBool){ 
        if(inputBool){
            speed_f = 0.26f;
        }else{
            speed_f = 0.24f;
            StopCoroutine(coroutine);//적 발견시 패트롤 종료
        }
        animator.SetBool("Static_b",false);// 가져온 컴퍼넌트의 Animator 컴포넌트의 컨트롤러에서 Walk Movement로 변하기위한 파라미터설정
        animator.SetFloat("Speed_f",speed_f);// walk의 파라미터중 하나인 Speed_f 도 설정
        animator.SetBool("Crouch_b",false);
    }

    //사격 애니메이션 제어 메서드
    public void shoot(bool inputBool){ 
        if(Camera.main.GetComponent<CamAudioController>().currentBgm == 1){
            Camera.main.GetComponent<CamAudioController>().PlaySound("Combat");
            Camera.main.GetComponent<CamAudioController>().currentBgm +=1;
        }
        if(inputBool){
            
            if(GetComponent<EnemyDetectionController>().gunType == "M5"){
                animator.SetInteger("WeaponType_int",1); //사격 모션용 무기 모션으로 바꾸기위해 WeaponType_int를 1로 설정
                animator.SetBool("Shoot_b",true);    //사격 모션으로 바꾸기위한 파라미터 설정.
            }else if (GetComponent<EnemyDetectionController>().gunType == "Shotgun"){
                animator.SetInteger("WeaponType_int",4); //사격 모션용 무기 모션으로 바꾸기위해 WeaponType_int를 1로 설정
                animator.SetBool("Shoot_b",true);    //사격 모션으로 바꾸기위한 파라미터 설정.
            }
            
        }else{
            animator.SetInteger("WeaponType_int",3); //라이플 모션으로 바꾸기위해 WeaponType_int를 3로 설정
            animator.SetBool("Shoot_b",false);  //사격 모션에서 평상 모션으로 바꾸기위한 파라미터 설정
            
        }
    }

    //무릎꿇기 애니메이션 제어 메서드
    public void Crouch(bool inputBool){
        if(inputBool){
            animator.SetBool("Crouch_b",true);  //무릎꿇는 모션으로 바꾸기위한 파라미터 설정
        }else{
            animator.SetBool("Crouch_b",false); //무릎꿇는 모션에서 평상 모션으로 바꾸기위한 파라미터 설정
        }
    }

    //타겟(플레이어)을 향해 이동하는 메서드
    public void goToTarget(Transform target){ //NavMeshAgent 제어 메서드
        if(!isDetouring){
            isDetouring = true;
            walk(true);
            Crouch(false);
            if(agent.enabled){
                agent.destination = target.position;
            }
            
            //agent.SetDestination(target.position);
        }
    }
    
    //정찰(패트롤)시 순찰 지점에 도달하면 다음 지점으로 이동시키는 정찰 제어 메서드
    void OnTriggerEnter(Collider coll){ //패트롤에 필요한 WayPoint 번호를 갱신
        if(wayPoints != null && coll.tag == "WayPoint") //WayPoint 태그를 단 gameObject와 닿을 시 실행됨
        {
            nextIdx = (++nextIdx >= wayPoints.Length) ? 1 : nextIdx;
        }
    }


    //Awake 함수에서 필드 초기화
    private void Awake()
    {
        tr = GetComponent<Transform>();
        wayPoints = null;
        agent = GetComponent<NavMeshAgent>();
        rigid = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>(); // 이 오브젝트가 가지고있는 Animator 컴포넌트 가져오기

        GameObject child = transform.GetChild(2).gameObject;
        sr = child.GetComponent<SkinnedMeshRenderer>();
        normalColor = sr.material.color;
    }

    // Update is called once per frame
    void Update()
    {
        if(wayPoints != null && agent.velocity.sqrMagnitude >= 0.5f  && agent.remainingDistance <= 4.5f){
                walk(false);
                Crouch(true);
                isDetouring = false;
        }
    }
    IEnumerator startPatrol()
    { // 패트롤을 실행시키는 코루틴 메서드
        isPatrolling = true;
        while (true)
        {
            yield return null;
            Patrolling();
        }
    }
    void Patrolling()
    { //정찰 지점으로 회전시키는 메서드
        Quaternion rot = Quaternion.LookRotation(wayPoints[nextIdx].position - tr.position);
        tr.rotation = Quaternion.Slerp(tr.rotation, rot, Time.deltaTime * damping);
        tr.Translate(Vector3.forward * Time.deltaTime * speed);
    }

    //정찰 포인트 배열 초기화 함수
    public void SetWayPoints(GameObject pointGroup)
    {
        wayPoints = pointGroup.GetComponentsInChildren<Transform>();
        coroutine = startPatrol();//로드시 패트롤 시작(코루틴 함수)
        walk(true); //걷기 애니메이션 시작
        animator.SetInteger("WeaponType_int", 3); //weapon에 따른 손 모양을 다르게하기 위한 애니메이터 파라미터 설정
        animator.SetBool("Reload_b", false); //shoot과 reload 애니로 안 넘어가게 false로 설정
        StartCoroutine(coroutine); //패트롤 시작
    }


}
