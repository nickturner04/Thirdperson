using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponButton : MonoBehaviour
{
    [HideInInspector]
    public Inventory inventory;
    public int weaponIndex;

    public void WeaponButtonClick()
    {
        transform.parent.gameObject.SetActive(false);
        inventory.Equip(weaponIndex);
    }
}
