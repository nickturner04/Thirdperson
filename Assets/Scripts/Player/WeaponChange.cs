using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponChange : MonoBehaviour
{
    [SerializeField] public GameObject[] weapons;
    [SerializeField] public bool[] acquired;
    [SerializeField] private Transform attachmentPoint;

    private void Unequip()
    {
        foreach (Transform child in attachmentPoint.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void Equip(int weapon)
    {
        Unequip();
        if (acquired[weapon])
        {
            Instantiate(weapons[weapon], attachmentPoint, false);
        }
    }
}
