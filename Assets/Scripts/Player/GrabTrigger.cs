using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabTrigger : MonoBehaviour
{
    //This class is attached to the hitbox for the player's grab radius, when an enemy enters the trigger it sends a message to the player controler

    private PlayerController playerController;

    private void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            playerController.SetGrabTarget(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            playerController.RemoveGrabTarget(other);
        }
    }
}
