using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DigController : MonoBehaviour
{
    [SerializeField] CaveController cave;
    [SerializeField] UIController mineralUI;
    [SerializeField] PlayerController player;
    [SerializeField] AudioSource miningAudio;
    [SerializeField] int width;
    [SerializeField] int height;

    private int focusingMineral = -1;
    private float focusingMineralMax;

    private const int COAL = 3;
    private const int IRON = 4;
    private const int GOLD = 5;
    private const int DIAMOND = 6;

    private void Awake()
    {
        cave = GameObject.FindAnyObjectByType<CaveController>();
        mineralUI = GameObject.FindAnyObjectByType<UIController>();
        player = GameObject.FindAnyObjectByType<PlayerController>();
        miningAudio = GetComponent<AudioSource>();
        width = cave.GetWidth();
        height = cave.GetHeight();
    }

    void Update()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        this.transform.position = mousePosition;

        player.SetPlayerSpeed(15.0f);
        if (Input.GetMouseButton(0) && focusingMineral != -1 && cave.GetMineralHP(focusingMineral) > 0 && CheckForBetweenObject())
        {
            if (!miningAudio.isPlaying) miningAudio.Play();
            player.SetPlayerSpeed(5.0f);
            float mineralHP = cave.GetMineralHP(focusingMineral);
            mineralHP -= Time.deltaTime;
            if (mineralHP <= 0)
            {
                mineralHP = 0.0f;
                int mouseX = (int)(mousePosition.x + width / 2), mouseY = (int)(mousePosition.y + height / 2);
                player.PlusMineralValue(cave.GetMineral(mouseX, mouseY) - COAL);
                cave.removeMineral(mouseX, mouseY, focusingMineral);
            }
            cave.SetMineralHP(focusingMineral, mineralHP);
            mineralUI.SetMineralHP(focusingMineralMax, mineralHP);
        }
        else miningAudio.Stop();
    }

    private bool CheckForBetweenObject()
    {
        Vector2 positionA = player.transform.position;
        Vector2 positionB = this.transform.position;
        Vector2 direction = (positionB - positionA).normalized;
        float distance = Vector2.Distance(positionA, positionB);

        // Raycast를 사용하여 타일이 있는지 검사
        RaycastHit2D[] hits = Physics2D.RaycastAll(positionA, direction, distance);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Player"))
                {
                    continue;
                }

                if (hit.collider.CompareTag("Wall"))
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Minerals"))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int mineralNumber = cave.GetMineralNumber((int)(mousePosition.x + width / 2.0f), (int)(mousePosition.y + height / 2.0f));
            mineralUI.SetUIActive(true);
            focusingMineral = mineralNumber;
            switch (cave.GetMineral((int)(mousePosition.x + width / 2), (int)(mousePosition.y + height / 2)))
            {
                case COAL: focusingMineralMax = 5.0f; break;
                case IRON: focusingMineralMax = 8.0f; break;
                case GOLD: focusingMineralMax = 10.0f; break;
                case DIAMOND: focusingMineralMax = 30.0f; break;
            }
            mineralUI.SetMineralHP(focusingMineralMax, cave.GetMineralHP(mineralNumber));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Minerals"))
        {
            focusingMineral = -1;
            focusingMineralMax = 0;
            mineralUI.SetUIActive(false);
        }
    }
}
