using UnityEngine;

public class CrazyMonsterMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // 괴물의 기본 이동 속도
    [SerializeField] private float changeDirectionTime = 2f; // 방향을 바꾸는 주기
    [SerializeField] private float maxShakeAmount = 1f; // 최대 흔들림 크기
    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private float elapsedTime = 0f;
    private Vector2 shakeOffset = Vector2.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Z축 회전 고정
        ChangeDirection(); // 초기 방향 설정
    }

    void Update()
    {
        elapsedTime += Time.deltaTime; // 경과 시간 업데이트

        if (elapsedTime >= changeDirectionTime)
        {
            ChangeDirection(); // 주기마다 방향 변경
            elapsedTime = 0f;
        }
    }

    void FixedUpdate()
    {
        // Rigidbody를 통해 괴물 이동
        rb.velocity = (moveDirection + shakeOffset) * moveSpeed;

        // 부가적인 움직임을 위해 랜덤한 흔들림을 생성
        shakeOffset = new Vector2(Random.Range(-maxShakeAmount, maxShakeAmount), Random.Range(-maxShakeAmount, maxShakeAmount));
    }

    void ChangeDirection()
    {
        // 무작위로 새로운 방향 생성
        float randomAngle = Random.Range(0f, 360f);
        moveDirection = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
    }
}
