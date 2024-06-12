using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f; // �÷��̾� �̵� �ӵ�

    private Rigidbody2D rb;
    private Vector2 movement;
    private DigController digController;
    private int[] minerals = new int[4];

    private void Start()
    {
        digController = GameObject.FindAnyObjectByType<DigController>();
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Z�� ȸ�� ����
    }

    private void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        Vector2 nextPosition = movement * moveSpeed * Time.fixedDeltaTime + rb.position;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - transform.position).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        rb.MovePosition(nextPosition);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Portal"))
        {
            collision.GetComponent<LevelObject>().MoveToNextLevel();
        }
    }

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    public void PlusMineralValue(int mineral)
    {
        minerals[mineral]++;
    }

    public void SetPlayerSpeed(float speed)
    {
        moveSpeed = speed;
    }
}
