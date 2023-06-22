using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage;
    public bool isMelee;
    public bool isRock;

    private void OnCollisionEnter(Collision collision)//콜라이더 상호작용함수
    {
        if(!isRock && collision.gameObject.tag == "Floor") { //Floor라는 태그에 닿으면
            Destroy(gameObject, 3);//3초후 삭제
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isMelee && other.gameObject.tag == "Wall") {
            Destroy(gameObject);
        }
    }
}
