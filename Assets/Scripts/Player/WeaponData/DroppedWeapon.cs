using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedWeapon : MonoBehaviour
{
    public WeaponData weaponData;
    public int ammo;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Dropped Weapon Start");
        Instantiate(weaponData.weaponModel,transform);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out PlayerController controller))
        {
            controller.SetDroppedWeapon(this);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out PlayerController controller) && controller.droppedWeapon == this)
        {
            controller.RemoveDroppedWeapon();
        }
    }
}
