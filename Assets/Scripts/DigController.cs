using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigController : MonoBehaviour
{
    private Collider2D collider2D;

    void Start()
    {
        // 게임 오브젝트에 부착된 2D 콜라이더를 가져옵니다.
        collider2D = GetComponent<Collider2D>();

        // 초기 상태로 콜라이더를 비활성화합니다.
        if (collider2D != null)
        {
            collider2D.enabled = false;
        }
    }


    void Update()
    {
        // 마우스 클릭을 감지합니다.
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Click TEST");
            collider2D.enabled = true;
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            this.transform.position = mousePosition;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collider2D.CompareTag("Minerals"))
            Debug.Log("Mineral TEST!");
    }
}
