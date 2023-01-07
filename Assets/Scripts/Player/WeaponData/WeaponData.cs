using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Weapon Data",menuName ="Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string displayName;
    public Sprite displaySprite;
    public float fireRate, resetTime, accuracy, reloadTime, damage;
    public int ammoType;
    public bool automatic;
    public int clipSize, maxHeat, baseHeat,numShots;
    public GameObject weaponModel;
    public AudioClip firesound;

    public virtual void Fire(Vector3 position,Vector3 direction)
    {

    }
}

