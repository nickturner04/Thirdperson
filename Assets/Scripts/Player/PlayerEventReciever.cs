using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEventReciever : MonoBehaviour
{


    private PlayerController controller;
    private GameManager manager;
    private SoundManager sound;
    private void Start()
    {
        controller = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        manager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        sound = GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>();
    }
    public void ReloadEnd()
    {
        controller.FinishReload();
    }
    public void StartDeath()
    {
        Debug.Log("died");
        GameObject.FindGameObjectWithTag("EnemyManager").GetComponent<AudioSource>().enabled = false;
        sound.PlayDeath();
        sound.GetComponent<AudioListener>().enabled = true;
    }
    public void Die()
    {
        manager.GameOver();
    }
}
