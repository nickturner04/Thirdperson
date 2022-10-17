using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupProp : MonoBehaviour
{
    public Rigidbody rb;
    private bool grabbed = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    public void Pickup()
    {
        rb.useGravity = false;
        rb.freezeRotation = true;
        rb.isKinematic = true;
        transform.localRotation = Quaternion.identity;
        gameObject.layer = 2;
    }

    public void PutDown()
    {
        rb.useGravity = true;
        rb.freezeRotation = false;
        rb.isKinematic = false;
        gameObject.layer = 6;
        transform.parent = null;
    }

    public void ToggleGrab()
    {
        if (grabbed)
        {
            PutDown();
        }
        else
        {
            Pickup();
        }
    }
}
