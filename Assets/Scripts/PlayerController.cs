using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // 플레이어 이동 속도

    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // 유저 회전 구현 시 삭제 요망
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {
        Vector2 nextPosition = movement * moveSpeed * Time.fixedDeltaTime + rb.position;
        rb.MovePosition(nextPosition);
    }
}
