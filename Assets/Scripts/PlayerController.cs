using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private Light2D playerFlashlight;
    [SerializeField] private Light2D playerLight;
    [SerializeField] private DigController digController;
    [SerializeField] private BaseController baseController;
    [SerializeField] private AudioSource stepAudio;
    [SerializeField] UIController mineralUI;

    private Rigidbody2D rb;
    private Vector2 movement;
    [SerializeField] private int[] minerals = new int[4];
    private bool flashlightPower = true;
    private void Start()
    {
        baseController = GameObject.FindAnyObjectByType<BaseController>();
        digController = GameObject.FindAnyObjectByType<DigController>();
        mineralUI = GameObject.FindAnyObjectByType<UIController>();
        stepAudio = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        if (SceneManager.GetActiveScene().name == "Base") return;
        
        if (movement.x == 0 && movement.y == 0) stepAudio.Stop();
        else if (!stepAudio.isPlaying) stepAudio.Play();

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
        if (collision.gameObject.CompareTag("Ghost") || collision.gameObject.CompareTag("Crazy"))
        {
            for (int i = 0; i < 4; i++) minerals[i] = 0;
            collision.gameObject.GetComponent<LevelObject>().MoveToNextLevel();
        }

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Portal"))
        {
            baseController.PlusMinerals(minerals);
            collision.GetComponent<LevelObject>().MoveToNextLevel();
        }
        if (collision.gameObject.CompareTag("Ghost") || collision.gameObject.CompareTag("Crazy"))
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
