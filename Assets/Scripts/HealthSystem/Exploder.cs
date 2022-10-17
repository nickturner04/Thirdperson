using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exploder : Damager
{
    public float radius;
    public float power;

    private void OnEnable() => Explode();

    private void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        foreach (var item in colliders)
        {
            if (item.TryGetComponent(out Health health))
            {
                Damage(health);
            }
            if (item.TryGetComponent(out Rigidbody rb))
            {
                Vector3 dir = (item.transform.position - transform.position).normalized;
                rb.AddForce(dir * power, ForceMode.Impulse);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
