using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Interactable : MonoBehaviour
{
    public string description;
    public UnityEvent OnInteract;
    public enum InteractionType
    {
        Click,
        Hold
    }

    public InteractionType interactionType;

    public abstract void Interact();
}
