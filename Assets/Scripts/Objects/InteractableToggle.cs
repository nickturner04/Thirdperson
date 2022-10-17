using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableToggle : Interactable
{
    public bool toggle;

    [SerializeField] private GameObject ToggleObject;
    public override void Interact()
    {
        toggle = !toggle;
        ToggleObject.SetActive(toggle);
        OnInteract.Invoke();
    }
}
