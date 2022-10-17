using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody),typeof(Health))]
public class VelocityDamager : Damager
{
    private Rigidbody rb;
    private Health health;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        health = GetComponent<Health>();
    }

    public float minDamageVelocity;
    [Range(0, 1)] public float velocityThreshold;

    private void OnCollisionEnter(Collision collision)
    {
        float damageFactor = rb.velocity.magnitude / minDamageVelocity;
        if (damageFactor > velocityThreshold)
            health.TakeDamage(damage * damageFactor);
    }
}
