using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupAmmo : MonoBehaviour
{
    [SerializeField] private int ammoType;
    [SerializeField] private int ammoAmmount;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Inventory inventory))
        {
            if (inventory.ammo[0,((int)ammoType)] != inventory.maxAmmo[((int)ammoType)])
            {
                inventory.AddAmmo(ammoAmmount, ammoType);
                gameObject.SetActive(false);
            }
        }
    }
}
