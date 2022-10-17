using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxDamager : Damager
{
    [SerializeField] private LayerMask layerMask;
    public void DamageBox()
    {
        Collider[] hits = Physics.OverlapBox(transform.position, transform.localScale, Quaternion.identity, layerMask);
        foreach (var item in hits)
        {
            if (item.TryGetComponent(out Health health))
            {
                Damage(health);
            }
        }
    }
}
