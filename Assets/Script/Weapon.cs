using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range }
    public Type type;
    public int damage; //���ط�
    public float rate; //����
    public int maxAmmo; //�ִ�źâ
    public int curAmmo; //�� źâ

    public BoxCollider meleeArea; //�ڽ��ݶ��̴� Ÿ���� ������Ʈ ���ݹ��� ���� ����
    public TrailRenderer trailEffect; //Ʈ���Ϸ����� Ÿ���� ������Ʈ ���� ����
    public Transform bulletPos; //�Ѿ� �߻� ��ġ ���� ���� 
    public GameObject bullet; //�Ѿ��� ������ ���� ����
    public Transform bulletCasePos; //ź�� ���� ��ġ ���� ���� 
    public GameObject bulletCase; //ź�Ǹ� ������ ���� ����


    public void Use() //���ݷ���(�ڷ�ƾ)
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
    IEnumerator Swing() //IEnumerator�� yield(��� ���� Ű����)�� �Ѱ� �̻� �ʿ�
    {
        yield return new WaitForSeconds(0.1f); //0.1�� ���
        meleeArea.enabled = true;
        trailEffect.enabled = true;
        
        yield return new WaitForSeconds(0.3f); //0.3�� ���
        meleeArea.enabled = false;

        yield return new WaitForSeconds(0.3f); //0.3�� ���
        trailEffect.enabled = false;

        yield break; //��� ���� ����
    }
    //�Ϲ��Լ�: Use() ���η�ƾ -> �����ƾ -> Use() ���η�ƾ
    //�ڷ�ƾ�Լ�: Use() ���η�ƾ + �����ƾ

    IEnumerator Shot()
    {
        //�Ѿ˹߻�
        GameObject intantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = intantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * 50;
        yield return null;
        //ź�ǹ���
        GameObject intantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody caseRigid = intantCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);
        caseRigid.AddForce(caseVec, ForceMode.Impulse);//ź�ǿ� ������ �� ���ϱ�
        caseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);//ź�ǿ� ȸ���ϴ� �� �ֱ�
    }

}
