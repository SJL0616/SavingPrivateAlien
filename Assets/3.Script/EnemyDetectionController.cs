using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 적 감지 컨트롤러
작성자: 이상준
내용:시야 내의 플레이어 감지, 3초간 응시, 사격모드 전환
     총 발사, 
     플레이어 방향으로 회전
마지막 수정일: 2022.6.22
*/
public class EnemyDetectionController : MonoBehaviour
{
   
    //시야 영역의 반지름과 시야 각도
    private float viewRadius;
    public float lookOutViewRadius;
    public float attackViewRadius;
    public float immediateCombatRadius;
    public float weaponRange;
    [Range(0, 360)]
    public float normalViewAngle;
    private float combatViewAngle = 360f;
    private float viewAngle;
    public float damping = 5.0f;
    public LayerMask targetMask, obstacleMask;
    private bool isLookOut = false;
    private bool isShooting = false;

    private bool isImmediateCombat = false;
    public bool isFreeToFire = false;
   //public GameObject holder;
   // public GameObject bullet;
    private int rayStack = 0;
    public List<Transform> visibleTargets = new List<Transform>();
    private Transform tr;
    private new EnemyAudioController audio;
    private EnemyMovementContorller move;
    private EnemyGunController gun;
    private GameManager manager;
    public string gunType;

    // 교전 모드 제어 메서드
    public void IntoCombatMode(){
        if(!isShooting){
            //GetComponent<EnemyMovementContorller>().shoot(true);
            isShooting = true;
            isImmediateCombat = true;
            isLookOut = true;
            //move.shoot(isFreeToFire);
        }
    }

    // 죽을 때 호출 메서드
    public void isDead(){
        audio.PlaySound("Dead");
        isShooting = false;
        isLookOut = false;
        StopCoroutine("FindTargetsDelay");
        Shooting(false);
        manager.leftEnemy -= 1;
        manager.LifeCalc();
        GameManager.currentScore += 1000;
    }

    void Start()
    { //플레이 시 FindTargetsDelay 코루틴을 실행한다. 0.5초 간격으로 
        tr = GetComponent<Transform>();
        StartCoroutine("FindTargetsDelay", 0.2f);
        audio = GetComponent<EnemyAudioController>();
        move = GetComponent<EnemyMovementContorller>();
        gun = GetComponent<EnemyGunController>();
        manager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        if(weaponRange <=20){
            gunType = "Shotgun";
        }else{
            gunType = "M5";
        }
    }

    // 적 탐지 코루틴 함수 
    IEnumerator FindTargetsDelay(float delay)
    {
        while (true)
        {   
           yield return new WaitForSeconds(delay); // 입력된 float 값 만큼 딜레이 있음
           FindTargets();
           if(isFreeToFire){
            Shooting(isShooting);
           }
        }
    }

    // 적 탐지 메서드
    //추적 메서드
    void FindTargets()
    {
        visibleTargets.Clear();

        if (isShooting)
        {                     // 사격 상태시
            viewRadius = attackViewRadius;  // 사격상태 일 때 시야 반지름을 길게(40)설정
            normalViewAngle = 360f;         //사격상태일 때 시야각을 360도로 설정
        }
        else
        {             //비사격 상태시
            viewRadius = lookOutViewRadius; // 기본 시야 반지름 적용
        }
        // viewRadius를 반지름으로 한 원 영역 내 targetMask 레이어인 콜라이더를 모두 가져옴
        Collider[] targetsInViewRadius;
        //즉각 사격 범위에 플레이어가 있는지 탐색
        Collider[] targetsInCombatRadius = Physics.OverlapSphere(transform.position, immediateCombatRadius, targetMask);
        if (targetsInCombatRadius.Length == 0)
        {//즉각 사격 범위에 없다면 기본 범위에서 탐색
            Collider[] targetsInLookOutRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
            targetsInViewRadius = targetsInLookOutRadius;
            viewAngle = normalViewAngle / 2;
        }
        else
        {                                //즉각사격 범위에 적이 있을 시
            targetsInViewRadius = targetsInCombatRadius;
            isImmediateCombat = true;
            viewAngle = combatViewAngle;
        }

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position);
            // 플레이어와 forward와 target이 이루는 각이 설정한 각도 내라면
            if (Vector3.Angle(transform.forward, dirToTarget) <= viewAngle)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.transform.position);
                // 타겟으로 가는 레이캐스트에 obstacleMask가 걸리지 않으면 visibleTargets에 Add
                RaycastHit hit;
                if (Physics.Raycast(transform.position, dirToTarget, out hit, dstToTarget))
                {
                    weaponRangeCal(dstToTarget, target);
                    if (!isShooting)
                    {//플레이어가 시야각에 있지만 공격상태가 아닌 경우 약 2초간 응시.
                        move.walk(false);
                        isLookOut = true;
                        Invoke("Identifying", 1.0f);
                    }
                    if (isImmediateCombat)
                    {//즉각 사격 상태일시
                        isImmediateCombat = false;
                        immediateCombatRadius = .0f;
                        isShooting = true;
                        isLookOut = true;
                    }
                    if (hit.transform.gameObject.layer != 3)
                    { //사이에 엄폐물 존재시
                        if (!move.isDetouring && !move.isBouncing && isShooting)
                        {
                            rayStack += 1;
                            if (rayStack >= 4)
                            { //일정 시간 후에 플레이어를 향해 이동 시작
                                move.goToTarget(target);
                                rayStack = 0;
                            }
                        }
                    }
                    else
                    {
                        rayStack = 0;
                        move.isDetouring = false;
                    }
                    visibleTargets.Add(target);
                }
            }
        }
    }

    //사격 범위 안의 적에게 이동 / 이동 중지 제어 메서드
    void weaponRangeCal(float dstToTarget,Transform target)
    {
        if(dstToTarget <= weaponRange){
            isFreeToFire = true;
        }
        else
        {
            isFreeToFire = false;
            if(target != null)
            {
                move.goToTarget(target);
            }
            
        }
    }

    //3초간 응시후 사격모드 코루틴화
    //적방향으로 회전 메서드
    void LookTarget(){
        Transform targetTr = visibleTargets[0];
        Quaternion  rot = Quaternion.LookRotation(targetTr.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * damping);
    }    

    // 적이 맞다면 사격 모드 전환 메서드
    void Identifying(){
        isShooting = true;                // shooting 메서드 시작
        immediateCombatRadius = .0f;
    }

    // 총알 발사 함수
    void Shooting(bool inputBool){
        gun.Shooting(gunType,inputBool);
    }

    // y축 오일러 각을 3차원 방향 벡터로 변환한다.
    public Vector3 DirFromAngle(float angleDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Cos((-angleDegrees + 90) * Mathf.Deg2Rad), 0, Mathf.Sin((-angleDegrees + 90) * Mathf.Deg2Rad));
    }
    
    // Update is called once per frame
    void Update()
    {
        //시야 내의 플레이어가 존재하면 플레이어를 향해 회전 함수 실행
        if (isLookOut && visibleTargets.Count > 0){ 
            LookTarget(); 
        }    
    }
}
