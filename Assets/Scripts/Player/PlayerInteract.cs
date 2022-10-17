using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerInteract : MonoBehaviour
{
    private PlayerController controller;
    void Start()
    {
        controller = GetComponent<PlayerController>();
    }

    void Update()
    {
        var getInteractable = GetInteractable();
        controller.interactable = getInteractable;
    }
    private Interactable GetInteractable()
    {
        var interactables = Physics.OverlapSphere(controller.trfPickup.position, 1, controller.interactableLayer);
        foreach (var item in interactables)
        {
            if (item.gameObject.TryGetComponent(out Interactable interactable))
            {
                return interactable;
            }
        }
        return null;
    }
}
