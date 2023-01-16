using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Weapons/Projectile Weapon")]
public class ProjectileWeaponData : WeaponData
{
    public GameObject projectile;
    public override void Fire(Vector3 position, Vector3 direction, float accuracymultiplier)
    {
        Instantiate(projectile,position,Quaternion.LookRotation(direction));
    }
}
