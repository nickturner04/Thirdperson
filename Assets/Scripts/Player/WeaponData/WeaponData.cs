using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Weapons/Weapon")]
public class WeaponData : ScriptableObject
{
    public string displayName;
    public Sprite displaySprite;
    public float fireRate, resetTime, accuracy, reloadTime, damage;
    public int ammoType;
    public int slot;
    public bool automatic;
    public int clipSize, maxHeat, baseHeat, numShots;
    public GameObject weaponModel;
    public GameObject decal;
    public AudioClip firesound;

    public virtual void Fire(Vector3 position, Vector3 direction, float accuracymultiplier)
    {
        for (int i = 0; i < numShots; i++)
        {
            //Add an element of randomness to each shot depending on how many shots have already been fired
            var angle = Quaternion.AngleAxis(Random.Range(-accuracy * accuracymultiplier, accuracy * accuracymultiplier), new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
            if (Physics.Raycast(position, angle * direction, out RaycastHit hit, Mathf.Infinity, -9))
            {

                //Debug.Log("Weapon Hit");
                if (hit.collider.gameObject.layer == 7)
                {//Create Bullet hole if terrain is hit
                    Instantiate(decal, hit.point, Quaternion.identity);
                }
                if (hit.collider.TryGetComponent(out Health health))
                {//Damage enemy if hits enemy
                    health.TakeDamage(damage);
                }
                if (hit.collider.TryGetComponent(out Rigidbody rb))
                {//Add force to physics object if hits physics object
                    Vector3 dir = (rb.transform.position - hit.point).normalized;
                    rb.AddForce(dir * 100);
                }
            }



        }
        
    }
}

