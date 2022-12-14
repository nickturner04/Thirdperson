using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private LabelManager labelManager;

    public WeaponData[] weapons = new WeaponData[3];
    public int currentWeapon { get; private set; }

    public int[,] ammo = 
        { { 90, 36, 120 },
          { 30, 8, 24 } };
    public int[] equippedAmmo = { 30, 8, 24 };
    public int[] reserveAmmo = { 90, 36, 120 };
    public int[] maxAmmo = { 90, 36, 120 };

    private void Start()
    {
        labelManager.vseWeaponButtons[0].clicked += Equip0;
        labelManager.vseWeaponButtons[1].clicked += Equip1;
        labelManager.vseWeaponButtons[2].clicked += Equip2;
    }

    private void Equip0()
    {
        Equip(0);
        labelManager.Inventory();
    }

    private void Equip1()
    {
        Equip(1);
        labelManager.Inventory();
    }

    private void Equip2()
    {
        Equip(2);
        labelManager.Inventory();
    }

    public void Equip(int index)
    {
        if (index == -1 || index == currentWeapon)
        {
            currentWeapon = -1;
            weaponController.Unequip();
        }
        else
        {
            currentWeapon = index;
            weaponController.Equip(weapons[index]);
            weaponController.currentReserve = ammo[0, index];
            weaponController.currentAmmo = ammo[1, index];
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
        labelManager.UpdateWeaponMenu(ammo,new Sprite[] { weapons[0].displaySprite, weapons[1].displaySprite, weapons[2].displaySprite });
        labelManager.ShowWeaponMenu();
    }

}
