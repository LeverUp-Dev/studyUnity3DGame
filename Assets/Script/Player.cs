using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{    
    public float speed;//속도 값 변수    
    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject[] grenades;//공전하는 물체를 컨트롤하기 위한 변수 생성(수류탄)
    public int hasGrenades;
    public GameObject grenadeObj;
    public Camera followCamera;
    public GameManager manager;

    public AudioSource jumpSound;

    public int ammo;
    public int coin;
    public int health;
    public int score;

    public int maxAmmo;// 위 변수의 최댓값을 넣을 변수들
    public int maxCoin;
    public int maxHealth;
    public int maxHasGrenades;

    float hAxis;// Input Axis 값을 받기 위한 변수 선언
    float vAxis;       
    
    bool wDown;//만든 Input을 넣을 변수 선언    
    bool jDown;//GetButton을 받기위한 변수 선언
    bool fDown;//공격
    bool gDown;//수류탄
    bool rDown;//장전
    bool iDown;//상호작용 변수
    bool sDown1, sDown2;//무기교체 변수

    bool isJump;//무한 점프를 막기위한 변수 선언
    bool isDodge;//무한 구르기를 막기위한 변수 선언
    bool isSwap;//스왑 시 스왑만 하도록 하는 변수 선언
    bool isReload;
    bool isFireReady = true;
    bool isBorder;
    bool isDamage; //무적타임 변수
    bool isShop;
    bool isDead;

    Vector3 moveVec;// 2 Axis 변수를 받아 움직일 수 있게 할 벡터 변수 선언 
    Vector3 dodgeVec;

    Rigidbody rigid;
    Animator anim;//Animator를 anim변수에 불러옴
    MeshRenderer[] meshs; //플레이어의 신체 정보를 가져오기 위한 배열 선언

    GameObject nearObject;//트리거 된 아이템 저장할 변수 선언
    public Weapon equipWeapon; 

    int equiWeaponIndex = -1;
    float fireDelay;

    void Awake()
    {   
        rigid = GetComponent<Rigidbody>();// 물리효과를 위해 선언후 초기화
        anim = GetComponentInChildren<Animator>();//Awake 함수에서 불러온 Animation을 초기화 
        meshs = GetComponentsInChildren<MeshRenderer>();//meshs 배열 초기화 복수를 가져오기 위해서는 GetComponent + s를 사용

        //PlayerPrefs.SetInt("MaxScore", 112500);//PlayerPrefs는 유니티에서 제공하는 간단한 세이브클래스
    }

    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Grenade();
        Attack();
        Reload();
        Dodge();
        Interation();
        Swap();
    }

    void GetInput()
    {
        //GetAxisRaw는 Axis값을 정수로 변환
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical"); //Input Manager에서 Horizontal과 vertical이 있음
                                              //Vector3(x축, y축, z축)으로 사용, normalized는 방향값이 1로 고정된 벡터 
        wDown = Input.GetButton("Walk"); //wDown 변수 초기와
        jDown = Input.GetButtonDown("Jump"); //jDown 변수 초기화
        fDown = Input.GetButton("Fire1"); //fDown 변수 초기화 (Fire1은 키가 기본 할당되어있음 마우스 왼쪽)
        gDown = Input.GetButtonDown("Fire2"); //수류탄 투척(마우스 오른쪽 키)
        rDown = Input.GetButtonDown("Reload"); //장전
        iDown = Input.GetButtonDown("Interation");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;// 대각선 이동을 위해 normalized사용

        if (isDodge)
            moveVec = dodgeVec;

        if (isSwap || !isFireReady || isDead)
            moveVec = Vector3.zero; 


        //(삼항 연산자 ? true 일 때 값 : false일 때 값)
        if(!isBorder)
        transform.position += moveVec * speed        
        * (wDown ? 0.3f : 1f) * Time.deltaTime;//transform 이동은 무조건 Time.deltaTime을 곱해 주어야 한다


        //변수 입력.SetBool("animator변수이름", 벡터값!=Vector3.zero); 사용공식
        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }
    void Turn()
    {
        //키보드에 의한 회전
        transform.LookAt(transform.position + moveVec);

        //마우스에 의한 회전
        if(fDown && !isDead)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);//스크린에서 ray를 쏘는 함수
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0f;
                transform.LookAt(transform.position + nextVec);
            }
        }
    }
    void Jump()
    {
        if(jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isSwap && !isDead) {
            rigid.AddForce(Vector3.up * 15, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;

            jumpSound.Play();
        }

    }
    void Grenade()
    {
        if (hasGrenades == 0)
            return;

        if(gDown && !isReload && !isSwap && !isDead) {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);//스크린에서 ray를 쏘는 함수
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 10;

                GameObject instantGrenade = Instantiate(grenadeObj, transform.position, transform.rotation); //함수로 수류탄 생성
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);

                hasGrenades--;
                grenades[hasGrenades].SetActive(false);
            }
        }
    }
    void Attack()
    {
        if (equipWeapon == null)
            return;

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if(fDown && isFireReady && !isDodge && !isSwap && !isShop && !isDead)
        {
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0;
        }
    }
    void Reload()
    {
        if (equipWeapon == null)
            return;

        if (equipWeapon.type == Weapon.Type.Melee)
            return;

        if (ammo == 0)
            return;

        if (rDown && !isJump && !isDodge && !isSwap && isFireReady && !isShop && !isDead)
        {
            anim.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut", 2.8f);
        }
    }
    void ReloadOut()
    {
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        equipWeapon.curAmmo = reAmmo;
        ammo -= reAmmo;
        isReload = false;
    }
    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge && !isSwap && !isShop && !isDead) {
            dodgeVec = moveVec;
            speed *= 2f;
            anim.SetTrigger("doDodge");
            isDodge = true;
            Invoke("DodgeOut", 0.6f); //Invoke는 시간차 함수이다.
        }
    }
    void DodgeOut() //시간차를 이용하여 구르기를 끝나게 하는 함수
    {
        speed *= 0.5f;
        isDodge = false;
    }
    void Swap()//무기를 손에 들 수 있게 만드는 함수
    {
        if (sDown1 && (!hasWeapons[0] || equiWeaponIndex == 0))
            return;
        if (sDown2 && (!hasWeapons[1] || equiWeaponIndex == 1))
            return;

        int weaponIndex = -1; //단축키에 맞는 무기를 배열에서 활성화
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;

        if ((sDown1 || sDown2) && !isJump && !isDodge && !isShop && !isDead) {
            if(equipWeapon != null) 
            equipWeapon.gameObject.SetActive(false);

            equiWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            anim.SetTrigger("doSwap");

            isSwap = true;
            Invoke("SwapOut", 0.4f); //Invoke는 시간차 함수이다.
        }
    }
    void SwapOut() //DodgeOut과 같은 원리로 종료 시간차 생성
    {
        isSwap = false;
    }

    void Interation() //상호작용함수
    {
        if (iDown && nearObject != null && !isJump && !isDodge && !isDead)
        {
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject); //사라지게 하는 함수
            }
            else if (nearObject.tag == "Shop")
            {
                Shop shop = nearObject.GetComponent<Shop>();
                shop.Enter(this);
                isShop = true;
            }
        }
    }

    void FreezeRotation()//자동회전방지
    {
        rigid.angularVelocity = Vector3.zero;
    }
    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 5 , Color.green);
        isBorder = Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall"));
    }
    void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }

    void OnCollisionEnter(Collision collision) //Collision가 지면과 충돌하면 발동하는 함수
    {
        if(collision.gameObject.tag == "Floor") {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Item") {
            Item item = other.GetComponent<Item>();//Item이라는 스크립트를 불러옴
            switch (item.type) {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if(ammo > maxAmmo)
                        ammo = maxAmmo;
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxCoin)
                        coin = maxCoin;
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxHealth)
                        health = maxHealth;
                    break;
                case Item.Type.Grenade:
                    grenades[hasGrenades].SetActive(true); //수류탄 개수만큼 공전체가 활성화
                    hasGrenades += item.value;
                    if (hasGrenades > maxHasGrenades)
                        hasGrenades = maxHasGrenades;
                    break;
            }
            Destroy(other.gameObject);
        }
        else if(other.tag == "EnemyBullet") {
            if (!isDamage) {
                Bullet enemyBullet = other.GetComponent<Bullet>(); //Bullet 스크립트 재활용
                health -= enemyBullet.damage; //적의 총알에 피격 당할 때 마다 체력 감소

                bool isBossAtk = other.name == "Boss Melee Area";
                StartCoroutine(OnDamage(isBossAtk)); //코루틴 시작
            }

            if (other.GetComponent<Rigidbody>() != null) //Rigidbody가 있는 총알만 사라지게 하기
                Destroy(other.gameObject);
        }
    }

    IEnumerator OnDamage(bool isBossAtk)
    {
        isDamage = true;
        foreach(MeshRenderer mesh in meshs) { //플레이어에게 대미지가 들어올 시 플레이어를 노란색으로 변경
            mesh.material.color = Color.yellow;
        }

        if (isBossAtk) //보스한테 타운트 당할 시 넉백 구현
            rigid.AddForce(transform.forward * -25, ForceMode.Impulse);

        if (health <= 0 && !isDead)
            OnDie();

        yield return new WaitForSeconds(1f); //무적 타임을 1초 부여

        isDamage = false;
        foreach (MeshRenderer mesh in meshs) {
            mesh.material.color = Color.white;
        }

        if (isBossAtk)
            rigid.velocity = Vector3.zero;
    }

    void OnDie()
    {
        anim.SetTrigger("doDie");
        isDead = true;
        manager.GameOver();
    }

    // 트리거 이벤트 Stay, Exit 사용(아이템 획득)
    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon" || other.tag == "Shop")//만약 Weapon 태그의 아이템에 몸이 닿으면
            nearObject = other.gameObject;// nearObject 변수에 other.gameObject를 저장
    }
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")//만약 영역에서 벗어나면
            nearObject = null;//nearObject를 비운다
        else if (other.tag == "Shop") {
            Shop shop = other.GetComponent<Shop>();
            shop.Exit();
            isShop = false;
            nearObject = null;
        }
    }
}
