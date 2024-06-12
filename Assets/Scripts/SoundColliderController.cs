using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundColliderController : MonoBehaviour
{
    [SerializeField] private AudioSource crazyMonsterSound;
    [SerializeField] private AudioSource ghostMonsterSound;
    [SerializeField] private GameObject player;
    private bool crazyMonsterSoundChecker = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        this.transform.position = player.transform.position;
        if (crazyMonsterSoundChecker && !crazyMonsterSound.isPlaying) crazyMonsterSound.Play();
        if (!crazyMonsterSoundChecker) crazyMonsterSound.Stop();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Crazy")) crazyMonsterSoundChecker = true;
        if (collision.gameObject.CompareTag("Ghost") && !ghostMonsterSound.isPlaying) ghostMonsterSound.Play();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Crazy")) crazyMonsterSoundChecker = false;
    }
}
