using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    [SerializeField] public float noiseLevel;
    [SerializeField] public Transform soundOrigin;
    [SerializeField] public float range;
    public float minFalloffDistance;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            other.GetComponent<FootstepCamoController>().SoundEmitters.Add(this);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            other.GetComponent<FootstepCamoController>().SoundEmitters.Remove(this);
        }
    }
}
