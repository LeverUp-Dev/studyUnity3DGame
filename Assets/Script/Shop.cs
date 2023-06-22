using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public RectTransform uiGroup; //UI
    public Animator anim; //애니메이션

    public GameObject[] itemObj;
    public int[] itemPrice;
    public Transform[] itemPos;
    public string[] talkData;
    public Text talkText;

    Player enterPlayer; //플레이어 받기

    public void Enter(Player player)
    {
        enterPlayer = player;
        uiGroup.anchoredPosition = Vector3.zero; //UI를 화면 정중앙으로 이동
    }

    public void Exit()
    {
        anim.SetTrigger("doHello"); //상점 종료 시 인사하기
        uiGroup.anchoredPosition = Vector3.down * 1000; //UI를 아래로 1000만큼 내림
    }

    public void Buy(int index)
    {
        int price = itemPrice[index];
        if(price > enterPlayer.coin) {
            StopCoroutine(Talk());
            StartCoroutine(Talk());
            return;
        }

        enterPlayer.coin -= price;
        Vector3 ranVec = Vector3.right * Random.Range(-3, 3)
                        + Vector3.forward *Random.Range(-3, 3);
        Instantiate(itemObj[index], itemPos[index].position + ranVec, itemPos[index].rotation);
    }

    IEnumerator Talk()
    {
        talkText.text = talkData[1];
        yield return new WaitForSeconds(2f);
        talkText.text = talkData[0];
    }
}
