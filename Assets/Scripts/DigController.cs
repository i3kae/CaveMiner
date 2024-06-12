using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigController : MonoBehaviour
{
    private Collider2D collider2D;

    void Start()
    {
        // ���� ������Ʈ�� ������ 2D �ݶ��̴��� �����ɴϴ�.
        collider2D = GetComponent<Collider2D>();

        // �ʱ� ���·� �ݶ��̴��� ��Ȱ��ȭ�մϴ�.
        if (collider2D != null)
        {
            collider2D.enabled = false;
        }
    }


    void Update()
    {
        // ���콺 Ŭ���� �����մϴ�.
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
