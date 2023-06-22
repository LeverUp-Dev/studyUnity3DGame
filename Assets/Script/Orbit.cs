using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour //궁국기 공전 구현
{
    public Transform target; //공전목표
    public float orbitSpeed; //공전속도
    Vector3 offset; //목표와의 거리
    
    void Start()
    {
        offset = transform.position - target.position; //공전하는 물체가 플레이어를 따라가게 구현
    }


    void Update()
    {
        transform.position = target.position + offset;
        transform.RotateAround(target.position,
                               Vector3.up,
                               orbitSpeed * Time.deltaTime);//transform.RotateAround 특정 물체를 공전하는 함수
        offset = transform.position - target.position;
    }
}
