using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] private float speed;
    private void Update()
    {
        gameObject.transform.Rotate(0, speed * Time.deltaTime, 0);
    }
}
