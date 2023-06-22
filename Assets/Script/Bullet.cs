using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage;
    public bool isMelee;
    public bool isRock;

    private void OnCollisionEnter(Collision collision)//�ݶ��̴� ��ȣ�ۿ��Լ�
    {
        if(!isRock && collision.gameObject.tag == "Floor") { //Floor��� �±׿� ������
            Destroy(gameObject, 3);//3���� ����
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isMelee && other.gameObject.tag == "Wall") {
            Destroy(gameObject);
        }
    }
}
