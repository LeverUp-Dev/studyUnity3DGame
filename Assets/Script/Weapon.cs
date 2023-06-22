using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range }
    public Type type;
    public int damage; //피해량
    public float rate; //공속
    public int maxAmmo; //최대탄창
    public int curAmmo; //현 탄창

    public BoxCollider meleeArea; //박스콜라이더 타입의 컴포넌트 공격범위 변수 생성
    public TrailRenderer trailEffect; //트라일랜더러 타입의 컴포넌트 변수 생성
    public Transform bulletPos; //총알 발사 위치 변수 생성 
    public GameObject bullet; //총알을 저장할 변수 생성
    public Transform bulletCasePos; //탄피 생성 위치 변수 생성 
    public GameObject bulletCase; //탄피를 저장할 변서 생성


    public void Use() //공격로직(코루틴)
    {
        if(type == Type.Melee)
        {
            StopCoroutine("Swing");
            StartCoroutine("Swing");
        }
        else if (type == Type.Range && curAmmo > 0)
        {
            curAmmo--;
            StartCoroutine("Shot");
        }
    }
    IEnumerator Swing() //IEnumerator는 yield(결과 전달 키워드)가 한개 이상 필요
    {
        yield return new WaitForSeconds(0.1f); //0.1초 대기
        meleeArea.enabled = true;
        trailEffect.enabled = true;
        
        yield return new WaitForSeconds(0.3f); //0.3초 대기
        meleeArea.enabled = false;

        yield return new WaitForSeconds(0.3f); //0.3초 대기
        trailEffect.enabled = false;

        yield break; //모든 실행 중지
    }
    //일반함수: Use() 메인루틴 -> 서브루틴 -> Use() 메인루틴
    //코루틴함수: Use() 메인루틴 + 서브루틴

    IEnumerator Shot()
    {
        //총알발사
        GameObject intantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = intantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * 50;
        yield return null;
        //탄피배출
        GameObject intantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody caseRigid = intantCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);
        caseRigid.AddForce(caseVec, ForceMode.Impulse);//탄피에 랜던한 힘 가하기
        caseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);//탄피에 회전하는 힘 넣기
    }

}
