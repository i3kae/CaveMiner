using UnityEngine;

public class CrazyMonsterMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // ������ �⺻ �̵� �ӵ�
    [SerializeField] private float changeDirectionTime = 2f; // ������ �ٲٴ� �ֱ�
    [SerializeField] private float maxShakeAmount = 1f; // �ִ� ��鸲 ũ��
    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private float elapsedTime = 0f;
    private Vector2 shakeOffset = Vector2.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Z�� ȸ�� ����
        ChangeDirection(); // �ʱ� ���� ����
    }

    void Update()
    {
        elapsedTime += Time.deltaTime; // ��� �ð� ������Ʈ

        if (elapsedTime >= changeDirectionTime)
        {
            ChangeDirection(); // �ֱ⸶�� ���� ����
            elapsedTime = 0f;
        }
    }

    void FixedUpdate()
    {
        // Rigidbody�� ���� ���� �̵�
        rb.velocity = (moveDirection + shakeOffset) * moveSpeed;

        // �ΰ����� �������� ���� ������ ��鸲�� ����
        shakeOffset = new Vector2(Random.Range(-maxShakeAmount, maxShakeAmount), Random.Range(-maxShakeAmount, maxShakeAmount));
    }

    void ChangeDirection()
    {
        // �������� ���ο� ���� ����
        float randomAngle = Random.Range(0f, 360f);
        moveDirection = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
    }
}
