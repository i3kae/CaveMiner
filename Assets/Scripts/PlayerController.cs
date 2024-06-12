using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f; // 플레이어 이동 속도
    [SerializeField] private Light2D playerFlashlight;
    [SerializeField] private Light2D playerLight;
    [SerializeField] private DigController digController;
    [SerializeField] UIController mineralUI;

    private Rigidbody2D rb;
    private Vector2 movement;
    private static int[] minerals = new int[4];
    private bool flashlightPower = true;
    private void Start()
    {
        digController = GameObject.FindAnyObjectByType<DigController>();
        mineralUI = GameObject.FindAnyObjectByType<UIController>();
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Z축 회전 고정
    }

    private void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        if (Input.GetMouseButtonDown(1))
        {
            flashlightPower = !flashlightPower;
            if (flashlightPower)
            {
                digController.enabled = true;
                playerFlashlight.gameObject.SetActive(true);
                playerLight.intensity = 1.0f;
                foreach (GhostMonsterController ghost in FindObjectsOfType<GhostMonsterController>())
                {
                    ghost.SetTargeting(true);
                }
            }
            else
            {
                digController.enabled = false;
                playerFlashlight.gameObject.SetActive(false);
                mineralUI.SetUIActive(false);
                playerLight.intensity = 0.2f;
                foreach (GhostMonsterController ghost in FindObjectsOfType<GhostMonsterController>())
                {
                    ghost.SetTargeting(false);
                }
            }
        }
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Portal"))
        {
            collision.gameObject.GetComponent<LevelObject>().MoveToNextLevel();
        }
        if (collision.gameObject.CompareTag("Monster"))
        {
            for (int i = 0; i < 4; i++) minerals[i] = 0;
            collision.gameObject.GetComponent<LevelObject>().MoveToNextLevel();
        }

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Portal"))
        {
            collision.GetComponent<LevelObject>().MoveToNextLevel();
        }
        if (collision.gameObject.CompareTag("Monster"))
        {
            for (int i = 0; i < 4; i++) minerals[i] = 0;
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
