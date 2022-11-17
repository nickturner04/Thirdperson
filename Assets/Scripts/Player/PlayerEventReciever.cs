using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEventReciever : MonoBehaviour
{
    private PlayerController controller;
    private void Start()
    {
        controller = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }
    public void ReloadEnd()
    {
        controller.FinishReload();
    }
}
