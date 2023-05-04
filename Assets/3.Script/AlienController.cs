using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
외계인(플레이어) 컨트롤러
작성자: 이상준
내용: 이동, 총 발사, 구르기 , 에이밍에 따른 회전, 애니메이션
마지막 수정일: 2022.6.22
*/
public class AlienController : MonoBehaviour
{
    public Camera mainCamera;
    public int HP = 100;
    public int score = 0;
    public int coin = 0;
    public GameObject holder;                     //총구 오브젝트
    private BulletPoolController bulletPool;      //오브젝트 풀 클래스 변수
    public bl_Joystick[] js;                      //조이스틱 클래스 변수
    public bl_Joystick moveJs;
    public bl_Joystick shootJs;
    private Animator animator;                    //animator 컴포넌트를 가져오기 위한 전역변수
    private Transform spine;                      // 아바타의 상체
    private Transform hips;
    private Transform rightLeg;
    private Vector3 target;                       // 상체 회전용 변수
    private Vector3 moveVecForDodge; 
    private int speed = 0;
    private float shootingStack = 0;
    private int bulletStack = 0 ;
    private bool isDodging = false;              //Dodge(회피) 애니메이션이 진행중임을 나타내는 bool값
    public bool isDodgeClicked = false;
    public bool isRunning = false;
    private bool isInvincible = false;            //무적 판정용 변수
    private bool isWalkingBack = false;
    private bool isReloading = false;
    private bool aimMode = false;
    private bool isClear = false;
    private Rigidbody rigid;
    
    public ParticleSystem particleObject1;
    public ParticleSystem runSmoke;
    public ParticleSystem baseSmoke;

    public AudioClip audioHit;
    public AudioClip audioYouDied;
    public AudioClip audioDodge;

    public GameObject barParent; 
    public GameObject reloadBar;   
    public GameObject reloadEndPoint; 
    AudioSource audioSource;
    GunAudioController gunAudioSource;
    GameManager manager;
    public Vector3 offset;
    // 콜라이더 충돌 처리 함수(OnCollisionEnter)
    // 적 병사의 무기 종류에 따라 다른 애니메이션, 데미지 처리를 하였음.
    void OnCollisionEnter(Collision collision)
    {
        if (!isInvincible && collision.gameObject.tag == "EnemyWeapon" && collision.gameObject.layer != 3)
        {
            onDamaged(1);
        }
    }
    void OnTriggerEnter(Collider collider)
    {
        if (!isInvincible && collider.gameObject.tag == "EnemyShotgun")
        {
            onDamaged(2);
        }
    }

    private void Awake()
    {
        mainCamera = Camera.main;
        mainCamera.GetComponent<Follow>().CamFollowStart(this.transform.gameObject);
        bulletPool = transform.GetComponentInChildren<BulletPoolController>();
        animator = GetComponent<Animator>();
        spine = animator.GetBoneTransform(HumanBodyBones.Spine); // 상체값 가져오기 (허리 위)
        hips = animator.GetBoneTransform(HumanBodyBones.Hips);

        gunAudioSource = holder.GetComponent<GunAudioController>();
        audioSource = GetComponent<AudioSource>();
        rigid = GetComponent<Rigidbody>();
        manager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        js = GameObject.FindObjectsOfType<bl_Joystick>();
        foreach(bl_Joystick it in js)
        {
            if (it.gameObject.name == "MoveJS")
            {
                moveJs = it;
            }
            else
            {
                shootJs = it;
            }
        }
    }


    void Start()
    {   
        //총알 오브젝트풀 초기화
        bulletPool.Initialize();
    }

    //Update 메서드 : 이동, 구르기 
    void Update()
    {
        //총알 장전시간을 나타내는  UI 위치를 플레이어 오브젝트 약간 위에 붙임.
        Vector3 nowPos = transform.position;
        barParent.transform.position = nowPos +offset;
        barParent.transform.LookAt(mainCamera.transform);

        //이동 로직 ( 눌린 방향키에 따라 Vector값 설정)
        Vector3 moveVec = new Vector3(0,0,0);

#if UNITY_EDITOR_WIN
        if (Input.GetKey(KeyCode.W))
        {
            moveVec -= new Vector3(1.0f, 0.0f, 0.0f); //위로 이동
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveVec += new Vector3(1.0f, 0.0f, 0.0f); //아래 이동
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveVec -= new Vector3(0.0f, 0.0f, 1.0f); //왼쪽 이동
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveVec += new Vector3(0.0f, 0.0f, 1.0f); //오른쪽이동
        }
        isRunning = Input.GetButton("Run");
#else
        moveVec = new Vector3(-moveJs.Vertical, 0, moveJs.Horizontal);
        moveVec.Normalize();
#endif

        //Left Shift 버튼으로 뛰기/걷기 전환 => 이동 속도와 애니메이션 전환
        speed = ( 
            moveVec == Vector3.zero ? 0 :
            isRunning ? 18 : 9
        );

        if(!isRunning || speed == 0){
            animator.SetTrigger("RunStop");
        }
        if(!isClear){ //게임 클리어 상태가 아닐 시
            if(!isDodging){//구르기 상태가 아니라면 이동
                rigid.velocity = (moveVec * speed) ;
                
                if(isWalkingBack){
                    speed = 8;
                }else{
                    this.transform.LookAt(transform.position + moveVec);
                }
                animator.SetInteger("move",speed);
                animator.SetBool("Aim",aimMode);
                animator.SetFloat("AimToRoll",1.0f);
                shooting(aimMode);
            }else{
                //구르기 상태라면 케릭터 기준 앞 방향으로만 빠르게 이동
                rigid.velocity = (moveVecForDodge * 18.0f);
                this.transform.LookAt(transform.position + moveVecForDodge);
            }
        }    
        
        particleController(speed);

#if UNITY_EDITOR_WIN
        //구르기(회피) 로직
        if (Input.GetKeyDown(KeyCode.Space) && !isDodging)//space바가 눌렸을 때 한번 반응하게 설정
        {
#else
        //구르기(회피) 로직
        if (isDodgeClicked && !isDodging )//space바가 눌렸을 때 한번 반응하게 설정
        {
#endif

            animator.SetBool("Dodge", true);// Dodge 애니 활성화
            PlaySound("Dodge");
            isInvincible = true;            //무적 상태 부여
            if(aimMode){
                StartCoroutine(aimModeOff());
                animator.SetFloat("AimToRoll",1.5f);
            }
            if(moveVec == Vector3.zero){
                moveVecForDodge = transform.forward;
            }else{
                moveVecForDodge = moveVec;
            }
            isDodging = true;

            if(runSmoke.isPlaying){
                runSmoke.Stop();
            }
            if(baseSmoke.isPlaying){
                baseSmoke.Stop();
            }
            if(!particleObject1.isPlaying){
                particleObject1.Play();
            }
            Invoke("DodgeEnd",0.4f);
        }
    }

    //에이밍에 따른 상체 회전
    void LateUpdate()
    {
#if UNITY_EDITOR_WIN
        if (Input.GetMouseButton(0) && !isDodging)  // 우클릭 눌리는 동안
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);  // 마우스가 클릭된 좌표를 구함
            RaycastHit raycastHit;
            if (Physics.Raycast(ray, out raycastHit, 100))
            {
                target = raycastHit.point - spine.position;              // 마우스 좌표까지의 방향 백터를 구함
            }
#else
        Vector3 shootVec = new Vector3(-shootJs.Vertical, 0, shootJs.Horizontal);
        if (shootVec != Vector3.zero && !isDodging){  // 우클릭 눌리는 동안
         target = shootVec;
#endif
                float angle = Mathf.Atan2(target.x, target.z) * Mathf.Rad2Deg; // Atan2 함수를 통해서 각도를 반환
            if(Vector3.Angle(transform.forward, target)<140 ){           // 케릭터 등 뒤 각도일 경우 무시
                spine.rotation = Quaternion.AngleAxis(angle,Vector3.up); // 케릭터 상체를 Y축 기준으로 회전시킴
            }else{
                isWalkingBack = true;                                    // 케릭터 등 뒤 각도일 경우 상체, 하체 회전
                spine.rotation = Quaternion.AngleAxis(angle,Vector3.up);
                transform.rotation = Quaternion.AngleAxis(angle,Vector3.up);    
            }
            
            StartCoroutine(aimModeOn());// 에임밍 애니메이션용 bool 변수 true 처리
       }
       else if(Input.GetMouseButtonUp(0)) // 우클릭을 때면 에이밍 애니메이션 끝냄
       {
            if(isWalkingBack){
                isWalkingBack = false;
            }
            StartCoroutine(aimModeOff());
       }
    }

    // 구르기 애니가 끝나면 기본 애니(edle)로 바꾸기 위한 함수
    void DodgeEnd()
    {  
        animator.SetBool("Dodge", false);
        isInvincible = false;
        moveVecForDodge = Vector3.zero;
        isDodging = false;
        if (!baseSmoke.isPlaying)
        {
            baseSmoke.Play();
        }
    }

    //승리시 위치와 애니메이션 실행 함수
    public void Victory(Vector3 nowPos)
    {
        StartCoroutine(VictorySetRotate(nowPos));
    }

    IEnumerator VictorySetRotate(Vector3 nowPos)
    {
        yield return null;
        animator.SetTrigger("Clear");
        isClear = true;
        transform.position = nowPos;

        Vector3 camPos = mainCamera.transform.position;
        Vector3 lookPos = new Vector3(camPos.x, transform.position.y, camPos.z);

        Vector3 dir = lookPos - transform.position;
        dir.Normalize();


        float time = Time.time;
        while (time + 2.0f >= Time.time)
        {
            yield return null;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.LookRotation(dir), Time.deltaTime * 0.9f);
        }
    }

    //이동 속도에 따른 부스터 파티클 조절.
    void particleController(int speed)
    {
        // 속도가 높다면 많은 양의 연기를 뿜는 파티클 활성화.
        if (speed == 0 || speed == 9)
        {
            if (runSmoke.isPlaying)
            {
                runSmoke.Stop();
            }
            if (!baseSmoke.isPlaying)
            {
                baseSmoke.Play();
            }
        }
        else
        {
            if (!runSmoke.isPlaying)
            {
                runSmoke.Play();
            }
            if (baseSmoke.isPlaying)
            {
                baseSmoke.Stop();
            }
        }
    }

    //총알 발사 함수
    void shooting(bool inputBool)
    {
        if (inputBool && !isReloading)
        {
            if (bulletStack <= 5)
            {
                shootingStack += Time.deltaTime;
                if (shootingStack >= 0.2f)
                {
                    gunAudioSource.PlayGunSound("Shot");
                    bulletStack++;
                    GameObject bullets = bulletPool.PopFromPool("rifle");
                    bullets.transform.position = holder.transform.position;
                    bullets.transform.rotation = spine.transform.rotation;
                    bullets.SetActive(true);
                    bullets.GetComponent<ProjectileMover>().shot();
                    Rigidbody rigid = bullets.GetComponent<Rigidbody>();
                    rigid.AddForce(spine.transform.forward * 50.0f, ForceMode.Impulse);
                    shootingStack = .0f;
                }
            }
            else
            {
                StartCoroutine("Reloading");
            }
        }
    }
    
    // 총알 재장전 코루틴 함수. 
    IEnumerator Reloading()
    {
        isReloading = true;
        barParent.SetActive(true);      // 재장전 bar형식으로 표시
        reloadBar.GetComponent<Animation>().Play();
        gunAudioSource.PlayGunSound("Reload");
        yield return new WaitForSeconds(1.9f);
        bulletStack = 0;
        isReloading = false;
        barParent.SetActive(false);
    }

    // 효과음 재생용 함수
    void PlaySound(string action) 
    {
        switch (action)
        {
            case "Hit":
                audioSource.clip = audioHit;
                audioSource.volume = 0.3f;
                break;
            case "Dodge":
                audioSource.clip = audioDodge;
                audioSource.volume = 1.0f;
                break;
            case "YouDied":
                audioSource.clip = audioYouDied;
                audioSource.volume = 0.5f;
                break;
        }
        audioSource.Play();
    }

    // 데미지 입는 함수
    void onDamaged(int type)
    {
        int damage = 0;
        if (type == 1)
        {
            damage = 20;
        }
        else
        {
            damage = 40;
        }
        PlaySound("Hit");
        animator.SetTrigger("Hit");
        HP -= damage;
        if (HP <= 0)
        {
            isInvincible = true;
            isClear = true;
            animator.SetTrigger("YouDied");
            PlaySound("YouDied");
            Invoke("Restart", 0.5f);
        }
    }

    // 플레이어가 죽었을 때 호출 함수
    void Restart()
    {
        moveJs.isFree = true;
        shootJs.isFree = true;
        manager.playerIsDead = true;
        manager.LifeCalc();
    }

    IEnumerator aimModeOn() // 에임모드가 켜지면 지정대기시간 이후 aimMode = true
    {
        yield return new WaitForSeconds(0f);
        aimMode = true;
    }

    IEnumerator aimModeOff() //에임모드가 꺼지면 지정대기시간이후 aimMode = false
    {
        yield return new WaitForSeconds(0f);
        aimMode = false;
    }
}
