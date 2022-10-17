using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{

    Transform mcamera;
    
    void Start()
    {
        mcamera = Camera.main.transform;
    }
    void Update()
    {
        transform.rotation = mcamera.rotation;
    }
}
