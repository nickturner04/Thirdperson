using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerEventReciever : MonoBehaviour
{


    private PlayerController controller;
    private GameManager manager;
    private SoundManager sound;
    private Gamepad gamepad;
    private void Start()
    {
        gamepad = Gamepad.current;
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
        gamepad.SetMotorSpeeds(1,1);
    }
    public void Die()
    {
        manager.GameOver();
        gamepad.SetMotorSpeeds(0, 0);
    }
    public void EndRoll()
    {
        controller.EndRoll();
        transform.position.Set(0, 0, 0);
    }
}
