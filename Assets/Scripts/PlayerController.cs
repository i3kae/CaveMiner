using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // 플레이어 이동 속도
    [SerializeField] private LayerMask wallLayer; // 벽 레이어
    [SerializeField] private float collisionBuffer = 2f; // 충돌 버퍼 거리

    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 입력 받기
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {
        // 이동할 위치 계산
        Vector2 newPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;

        // 플레이어 크기 및 충돌 버퍼를 고려하여 레이캐스트 길이 설정
        float raycastLength = movement.magnitude * moveSpeed * Time.fixedDeltaTime + collisionBuffer;

        // 벽과의 충돌을 확인하기 위해 레이캐스트 사용
        RaycastHit2D hit = Physics2D.Raycast(rb.position, movement, raycastLength, wallLayer);

        // 벽과 충돌하지 않는 경우에만 이동
        if (hit.collider == null)
        {
            rb.MovePosition(newPosition);
        }
        else
        {
            // 벽과 충돌한 경우 이동 중지 또는 다른 처리
            rb.MovePosition(rb.position); // 현재 위치 유지
        }
    }
}
