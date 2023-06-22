using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{    
    public float speed;//�ӵ� �� ����    
    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject[] grenades;//�����ϴ� ��ü�� ��Ʈ���ϱ� ���� ���� ����(����ź)
    public int hasGrenades;
    public GameObject grenadeObj;
    public Camera followCamera;
    public GameManager manager;

    public AudioSource jumpSound;

    public int ammo;
    public int coin;
    public int health;
    public int score;

    public int maxAmmo;// �� ������ �ִ��� ���� ������
    public int maxCoin;
    public int maxHealth;
    public int maxHasGrenades;

    float hAxis;// Input Axis ���� �ޱ� ���� ���� ����
    float vAxis;       
    
    bool wDown;//���� Input�� ���� ���� ����    
    bool jDown;//GetButton�� �ޱ����� ���� ����
    bool fDown;//����
    bool gDown;//����ź
    bool rDown;//����
    bool iDown;//��ȣ�ۿ� ����
    bool sDown1, sDown2;//���ⱳü ����

    bool isJump;//���� ������ �������� ���� ����
    bool isDodge;//���� �����⸦ �������� ���� ����
    bool isSwap;//���� �� ���Ҹ� �ϵ��� �ϴ� ���� ����
    bool isReload;
    bool isFireReady = true;
    bool isBorder;
    bool isDamage; //����Ÿ�� ����
    bool isShop;
    bool isDead;

    Vector3 moveVec;// 2 Axis ������ �޾� ������ �� �ְ� �� ���� ���� ���� 
    Vector3 dodgeVec;

    Rigidbody rigid;
    Animator anim;//Animator�� anim������ �ҷ���
    MeshRenderer[] meshs; //�÷��̾��� ��ü ������ �������� ���� �迭 ����

    GameObject nearObject;//Ʈ���� �� ������ ������ ���� ����
    public Weapon equipWeapon; 

    int equiWeaponIndex = -1;
    float fireDelay;

    void Awake()
    {   
        rigid = GetComponent<Rigidbody>();// ����ȿ���� ���� ������ �ʱ�ȭ
        anim = GetComponentInChildren<Animator>();//Awake �Լ����� �ҷ��� Animation�� �ʱ�ȭ 
        meshs = GetComponentsInChildren<MeshRenderer>();//meshs �迭 �ʱ�ȭ ������ �������� ���ؼ��� GetComponent + s�� ���

        //PlayerPrefs.SetInt("MaxScore", 112500);//PlayerPrefs�� ����Ƽ���� �����ϴ� ������ ���̺�Ŭ����
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
        //GetAxisRaw�� Axis���� ������ ��ȯ
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical"); //Input Manager���� Horizontal�� vertical�� ����
                                              //Vector3(x��, y��, z��)���� ���, normalized�� ���Ⱚ�� 1�� ������ ���� 
        wDown = Input.GetButton("Walk"); //wDown ���� �ʱ��
        jDown = Input.GetButtonDown("Jump"); //jDown ���� �ʱ�ȭ
        fDown = Input.GetButton("Fire1"); //fDown ���� �ʱ�ȭ (Fire1�� Ű�� �⺻ �Ҵ�Ǿ����� ���콺 ����)
        gDown = Input.GetButtonDown("Fire2"); //����ź ��ô(���콺 ������ Ű)
        rDown = Input.GetButtonDown("Reload"); //����
        iDown = Input.GetButtonDown("Interation");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;// �밢�� �̵��� ���� normalized���

        if (isDodge)
            moveVec = dodgeVec;

        if (isSwap || !isFireReady || isDead)
            moveVec = Vector3.zero; 


        //(���� ������ ? true �� �� �� : false�� �� ��)
        if(!isBorder)
        transform.position += moveVec * speed        
        * (wDown ? 0.3f : 1f) * Time.deltaTime;//transform �̵��� ������ Time.deltaTime�� ���� �־�� �Ѵ�


        //���� �Է�.SetBool("animator�����̸�", ���Ͱ�!=Vector3.zero); ������
        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }
    void Turn()
    {
        //Ű���忡 ���� ȸ��
        transform.LookAt(transform.position + moveVec);

        //���콺�� ���� ȸ��
        if(fDown && !isDead)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);//��ũ������ ray�� ��� �Լ�
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
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);//��ũ������ ray�� ��� �Լ�
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 10;

                GameObject instantGrenade = Instantiate(grenadeObj, transform.position, transform.rotation); //�Լ��� ����ź ����
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
            Invoke("DodgeOut", 0.6f); //Invoke�� �ð��� �Լ��̴�.
        }
    }
    void DodgeOut() //�ð����� �̿��Ͽ� �����⸦ ������ �ϴ� �Լ�
    {
        speed *= 0.5f;
        isDodge = false;
    }
    void Swap()//���⸦ �տ� �� �� �ְ� ����� �Լ�
    {
        if (sDown1 && (!hasWeapons[0] || equiWeaponIndex == 0))
            return;
        if (sDown2 && (!hasWeapons[1] || equiWeaponIndex == 1))
            return;

        int weaponIndex = -1; //����Ű�� �´� ���⸦ �迭���� Ȱ��ȭ
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
            Invoke("SwapOut", 0.4f); //Invoke�� �ð��� �Լ��̴�.
        }
    }
    void SwapOut() //DodgeOut�� ���� ������ ���� �ð��� ����
    {
        isSwap = false;
    }

    void Interation() //��ȣ�ۿ��Լ�
    {
        if (iDown && nearObject != null && !isJump && !isDodge && !isDead)
        {
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject); //������� �ϴ� �Լ�
            }
            else if (nearObject.tag == "Shop")
            {
                Shop shop = nearObject.GetComponent<Shop>();
                shop.Enter(this);
                isShop = true;
            }
        }
    }

    void FreezeRotation()//�ڵ�ȸ������
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

    void OnCollisionEnter(Collision collision) //Collision�� ����� �浹�ϸ� �ߵ��ϴ� �Լ�
    {
        if(collision.gameObject.tag == "Floor") {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Item") {
            Item item = other.GetComponent<Item>();//Item�̶�� ��ũ��Ʈ�� �ҷ���
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
                    grenades[hasGrenades].SetActive(true); //����ź ������ŭ ����ü�� Ȱ��ȭ
                    hasGrenades += item.value;
                    if (hasGrenades > maxHasGrenades)
                        hasGrenades = maxHasGrenades;
                    break;
            }
            Destroy(other.gameObject);
        }
        else if(other.tag == "EnemyBullet") {
            if (!isDamage) {
                Bullet enemyBullet = other.GetComponent<Bullet>(); //Bullet ��ũ��Ʈ ��Ȱ��
                health -= enemyBullet.damage; //���� �Ѿ˿� �ǰ� ���� �� ���� ü�� ����

                bool isBossAtk = other.name == "Boss Melee Area";
                StartCoroutine(OnDamage(isBossAtk)); //�ڷ�ƾ ����
            }

            if (other.GetComponent<Rigidbody>() != null) //Rigidbody�� �ִ� �Ѿ˸� ������� �ϱ�
                Destroy(other.gameObject);
        }
    }

    IEnumerator OnDamage(bool isBossAtk)
    {
        isDamage = true;
        foreach(MeshRenderer mesh in meshs) { //�÷��̾�� ������� ���� �� �÷��̾ ��������� ����
            mesh.material.color = Color.yellow;
        }

        if (isBossAtk) //�������� Ÿ��Ʈ ���� �� �˹� ����
            rigid.AddForce(transform.forward * -25, ForceMode.Impulse);

        if (health <= 0 && !isDead)
            OnDie();

        yield return new WaitForSeconds(1f); //���� Ÿ���� 1�� �ο�

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

    // Ʈ���� �̺�Ʈ Stay, Exit ���(������ ȹ��)
    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon" || other.tag == "Shop")//���� Weapon �±��� �����ۿ� ���� ������
            nearObject = other.gameObject;// nearObject ������ other.gameObject�� ����
    }
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")//���� �������� �����
            nearObject = null;//nearObject�� ����
        else if (other.tag == "Shop") {
            Shop shop = other.GetComponent<Shop>();
            shop.Exit();
            isShop = false;
            nearObject = null;
        }
    }
}
