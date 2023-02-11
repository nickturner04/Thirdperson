using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private LabelManager labelManager;
    [SerializeField] private GameObject droppedWeaponPrefab;

    public WeaponData[] weapons = new WeaponData[3];
    public int currentWeapon { get; private set; }

    public int[,] ammo = 
        { { 90, 36, 120 },
          { 30, 8, 24 } };
    public int[] currentAmmo = { 30, 8, 24 };
    public int[] reserveAmmo = { 90, 36, 120,8 };
    public int[] maxAmmo = { 90, 36, 120, 8 };

    private void Start()
    {
        labelManager.vseWeaponButtons[0].clicked += Equip0;
        labelManager.vseWeaponButtons[1].clicked += Equip1;
        labelManager.vseWeaponButtons[2].clicked += Equip2;
    }

    private void Equip0()
    {
        if (currentWeapon == 0)
        {
            Equip(-1);
        }
        else
        {
            Equip(0);
            labelManager.Inventory();
        }
    }

    private void Equip1()
    {
        if (currentWeapon == 1)
        {
            Equip(-1);
        }
        else
        {
            Equip(1);
            labelManager.Inventory();
        }
    }

    private void Equip2()
    {
        if (currentWeapon == 2)
        {
            Equip(-1);
        }
        else
        {
            Equip(2);
            labelManager.Inventory();
        }
    }

    public void Equip(int index)
    {
        if (index == -1 )
        {
            currentWeapon = -1;
            weaponController.Unequip();
        }
        else
        {
            currentWeapon = index;
            if (weapons[index] == null)
            {
                currentWeapon = -1;
                weaponController.Unequip();
            }
            else
            {
                weaponController.Equip(weapons[index]);
                weaponController.currentReserve = ammo[0, index];
                weaponController.currentAmmo = ammo[1, index];
            }
            
        }
        
    }

    public void ChangeWeapon(WeaponData newWeapon,int ammo)
    {
        if (weapons[newWeapon.slot] != null)
        {
            var droppos = transform.position + transform.forward * 3;
            droppos.y *= 2;
            var droppedweapon = Instantiate(droppedWeaponPrefab, transform.position, Quaternion.identity).GetComponentInChildren<DroppedWeapon>();
            droppedweapon.ammo = currentAmmo[newWeapon.slot];
            droppedweapon.weaponData = weapons[newWeapon.slot];
        }

        currentAmmo[newWeapon.slot] = ammo;
        weapons[newWeapon.slot] = newWeapon;
        if (currentWeapon == newWeapon.slot)
        {
            Equip(newWeapon.slot);
        }
    }

    public void AddAmmo(int amount,int type)
    {
        ammo[0, type] += amount;
        if (ammo[0, type] > maxAmmo[type])
        {
            ammo[0, type] = maxAmmo[type];
            audioSource.PlayOneShot(pickupSound);
        }
    }

    public void Menu()
    {
        string[] ammo = new string[3];
        Sprite[] sprites = new Sprite[3];
        for (int i = 0; i < 3; i++)
        {
            if (weapons[i] == null)
            {
                ammo[i] = "NONE";
                sprites[i] = null;
            }
            else
            {
                ammo[i] = $"{currentAmmo[i]}/{reserveAmmo[weapons[i].ammoType]}";
                sprites[i] = weapons[i].displaySprite;
            }
        }
        labelManager.UpdateWeaponMenu(ammo,sprites);
        labelManager.ShowWeaponMenu();
    }

}
