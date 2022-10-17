using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [SerializeField] private GameObject decal;

    private float speed = 50;
    private float timeToDestroy = 3;

    public Vector3 target { get; set; }
    public bool hit { get; set; }

    private void OnEnable()
    {
        Destroy(gameObject, timeToDestroy);
    }
 
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        if (!hit && Vector3.Distance(transform.position, target) < .01f)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        var contact = collision.GetContact(0);
        Instantiate(decal, contact.point + contact.normal * 0.0001f, Quaternion.LookRotation(contact.normal), transform.parent);
        Destroy(gameObject);
    }
}
