using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // �÷��̾� �̵� �ӵ�
    [SerializeField] private LayerMask wallLayer; // �� ���̾�
    [SerializeField] private float collisionBuffer = 2f; // �浹 ���� �Ÿ�

    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // �Է� �ޱ�
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {
        // �̵��� ��ġ ���
        Vector2 newPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;

        // �÷��̾� ũ�� �� �浹 ���۸� ����Ͽ� ����ĳ��Ʈ ���� ����
        float raycastLength = movement.magnitude * moveSpeed * Time.fixedDeltaTime + collisionBuffer;

        // ������ �浹�� Ȯ���ϱ� ���� ����ĳ��Ʈ ���
        RaycastHit2D hit = Physics2D.Raycast(rb.position, movement, raycastLength, wallLayer);

        // ���� �浹���� �ʴ� ��쿡�� �̵�
        if (hit.collider == null)
        {
            rb.MovePosition(newPosition);
        }
        else
        {
            // ���� �浹�� ��� �̵� ���� �Ǵ� �ٸ� ó��
            rb.MovePosition(rb.position); // ���� ��ġ ����
        }
    }
}
