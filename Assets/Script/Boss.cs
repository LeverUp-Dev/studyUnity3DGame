using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss : Enemy //Awake는 상속되지 않음
{
    public GameObject missile;
    public Transform missilePortA;
    public Transform missilePortB;
    public bool isLook;

    Vector3 lookVec; //플레이어의 움직임을 예측하는 값을 저장하는 변수
    Vector3 tauntVec; //타운트를 저장할 변수

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        nav.isStopped = true;
        StartCoroutine(Think());
    }

    void Update()
    {
        if(isDead) { 
            StopAllCoroutines();
            return;
        }

        if (isLook) {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            lookVec = new Vector3(h, 0, v) * 5f; //플레이어의 움직임을 5칸 예측
            transform.LookAt(target.position + lookVec);
        }
        else
            nav.SetDestination(tauntVec);
    }

    IEnumerator Think()
    {
        yield return new WaitForSeconds(0.1f);

        int ranAction = Random.Range(0, 5);
        switch(ranAction) { 
            case 0:            
            case 1:
                StartCoroutine(MissileShot());
                break;//미사일 발사 패턴
            case 2:
            case 3:
                StartCoroutine(RockShot());
                break;//돌 던지기 패턴
            case 4:
                StartCoroutine(Taunt());
                break;//점프 공격 패턴
        }
    }

    IEnumerator MissileShot()
    {
        anim.SetTrigger("doShot");
        yield return new WaitForSeconds(0.2f);
        GameObject instantMissileA = Instantiate(missile, missilePortA.position, missilePortA.rotation);//함수로 미사일 생성
        BossMissile bossMissileA = instantMissileA.GetComponent<BossMissile>();
        bossMissileA.target = target;//미사일 스크립트까지 접근하여 목표물 설정해 주기

        yield return new WaitForSeconds(0.3f);
        GameObject instantMissileB = Instantiate(missile, missilePortB.position, missilePortB.rotation);//함수로 미사일 생성
        BossMissile bossMissileB = instantMissileB.GetComponent<BossMissile>();
        bossMissileB.target = target;//미사일 스크립트까지 접근하여 목표물 설정해 주기

        yield return new WaitForSeconds(2f);

        StartCoroutine(Think());//재귀
    }

    IEnumerator RockShot()
    {
        isLook = false;
        anim.SetTrigger("doBigShot");
        Instantiate(bullet, transform.position, transform.rotation);
        yield return new WaitForSeconds(3f);

        isLook = true;
        StartCoroutine(Think());//재귀
    }

    IEnumerator Taunt()
    {
        tauntVec = target.position + lookVec; //점프 후 착지 지점

        isLook = false;
        nav.isStopped = false;
        boxCollider.enabled = false;
        anim.SetTrigger("doTaunt");

        yield return new WaitForSeconds(1.5f);
        meleeArea.enabled = true;

        yield return new WaitForSeconds(0.5f);
        meleeArea.enabled = false;

        yield return new WaitForSeconds(1f);
        isLook = true;
        nav.isStopped = true;
        boxCollider.enabled = true;
        StartCoroutine(Think());//재귀
    }
}
