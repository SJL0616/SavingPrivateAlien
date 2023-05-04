using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public GameObject[] weapons;//무기 변수 선언
    public bool[] hasWeapons;//무기 집었을때 bool변수 선언


    public int ammo;
    public int coin;
    public int health;
    public int score;

    public int maxAmmo;
    public int maxCoin;
    public int maxHealth;
    public int maxHasGrenades;



    
    float hAxis; //수평이동 변수
    float vAxis; //수직이동 변수
   
    bool wDown; //워크 bool 선언
    bool jDown; //점프 변수 선언
    bool fDown;
    bool gDown;
    bool rDown;
    bool iDown;//아이템 다운 변수 선언
    bool sDown1;
    bool sDown2;
    bool sDown3;
    

    bool isJump;//점프에 제약을 주기 위한 변수 선언
    bool isDodge;//회피 변수 선언
    bool isSwap;
    bool isReload;
    //bool isFireReady = true;
    bool isBorder;
    bool isDamage;
    bool isShop;


    Vector3 moveVec; //h와v 위 변수 두개를 합칠 변수선언
    Vector3 dodgevec; // 회피하는 동안 움직일수 없게 하는 변수
    Rigidbody rigid; //중력적용
    Animator anim; // 애니메이션
    MeshRenderer[] meshes;

    GameObject nearObject; //트리거 된 아이템을 저장하기 위한 변수 선언
    //public Weapon equipWeapon;
    //int eqiupWeaponIndex = -1;
    float fireDelay;


    void Awake() //초기화는 Awake에서 한다.
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        //meshs = GetComponentInChildren<MeshRenderer>();

        Debug.Log(PlayerPrefs.GetInt("MaxScore"));
        //PlayerPrefs.SetInt("MaxScore", 723423);
    }

    
    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Dodge();
        Interation();

    }

    void GetInput()//입력을 받으면 움직이는 함수이다.
    {
        //GetAxisRaw():Axis 값을 정수로 반환하는 함수 - 키보드를 눌렀다 땠다 하는 순간 0.1 로 변경된다.
        hAxis = Input.GetAxisRaw("Horizontal");//Horizontal 과 Vertical 은 Project Setting의 Input Manager에서 관리되고 설정되어 있다.
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        iDown = Input.GetButtonDown("Interation");
    }

    void Move()//캐릭터가 상하좌우 피타고라스함수로 움직이는 함수이다.
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        
        if (wDown)
            transform.position += moveVec * speed * 0.3f * Time.deltaTime;
        else
            transform.position += moveVec * speed * Time.deltaTime; //강의에서 3항 연산자는 에러가 나서 if-else 구문을 사용

        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }
    void Turn() //키보드 두개 눌렀을시 회전하는 함수 이다.
    {
        transform.LookAt(transform.position + moveVec);//계속회전하는 에러를 잡는 우리가 바라본 방향으로 본다.
    }

    void Jump() //키보드가 점프하는 함수이다.
    {
        if(jDown && moveVec == Vector3.zero  && !isJump && !isDodge)
        {
            rigid.AddForce(Vector3.up * 5, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }
    void Dodge() //회피에 사용하는 함수이다.
    {
        if (jDown && jDown && moveVec != Vector3.zero && !isJump && !isDodge)
        {
                 
        
            speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.5f);
        }
    }

    void DodgeOut() 
    {
        speed *= 0.5f;
        isDodge = false;
    }

    void Interation() 
    { 
        if(iDown && nearObject != null && !isJump && !isDodge) 
        { 
            if(nearObject.tag == "Weapon")
            { 
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject);
            }
        }
    }

    
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor") 
        {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }

    void OnTriggerStay(Collider other)//영역에 들어왔을때 아이템 먹는 스크립트
    {
        if(other.tag == "Weapon" || other.tag == "Shop")
           nearObject = other.gameObject;
        Debug.Log(nearObject.name);//스크립트 로그 확인
    }

    void OnTriggerExit(Collider other)//영역을 벗어났을때 아이템 포기 스크립트
    {
        if (other.tag == "Weapon")
            nearObject = null;

    }
}
