using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableNormal : Interactable
{
    public override void Interact()
    {
        OnInteract.Invoke();
    }
}
