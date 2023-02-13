using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadiusAttack : MonoBehaviour
{
    [SerializeField] private float damage;
    [SerializeField] private float minFalloff;
    [SerializeField] private float maxFalloff;
    private const int terrainMask = 1 << 7;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Health health) && !health.isPlayer)
        {
            var dist = Vector3.Distance(transform.position, health.transform.position);
            if (!Physics.Raycast(transform.position,health.transform.position,dist,terrainMask))
            {
                if (dist <= minFalloff) health.TakeDamage(damage);
                else
                {
                    health.TakeDamage(damage * (1 - dist / maxFalloff));
                }
            }
            
        }
    }
}
