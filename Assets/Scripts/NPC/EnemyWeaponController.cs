using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeaponController : MonoBehaviour
{
    [HideInInspector]
    private WeaponAnim weaponAnim;
    private GameObject weaponObject;
    private WeaponData weaponData;
    private PlayerHealth playerHealth;
    private Transform player;

    [SerializeField] private GameObject decal;
    [SerializeField] private Transform attachPoint;
    [SerializeField] private WeaponData[] weapons;
    [SerializeField] private Transform AimTarget;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Transform firePoint;

    public int currentAmmo, clipSize, baseHeat;
    private bool firing = false;
    private int maxHeat, heat, numShots;
    private float fireRate, resetTime, accuracy, damage;
    private AudioClip fireSound;

    private float currentTimeToFire, currentResetTime = 0;

    private void Start()
    {
        playerHealth = GameObject.Find("PlayerV5").GetComponent<PlayerHealth>();
        player = playerHealth.transform;
        Equip(weapons[0]);
    }

    public void Equip(WeaponData weapon)
    {
        weaponData = weapon;
        maxHeat = weapon.maxHeat;
        fireRate = weapon.fireRate;
        resetTime = weapon.resetTime;
        accuracy = weapon.accuracy;
        damage = weapon.damage;
        clipSize = weapon.clipSize;
        numShots = weapon.numShots;
        baseHeat = weapon.baseHeat;

        weaponObject = Instantiate(weapon.weaponModel, attachPoint);
        weaponAnim = weaponObject.GetComponent<WeaponAnim>();
        fireSound = weapon.firesound;
        currentTimeToFire = 0;
        currentAmmo = clipSize;
    }

    public void Fire(bool firing)
    {
        this.firing = firing;
    }

    public void EndFire()
    {
        firing = false;
    }

    public void Update()
    {
        currentTimeToFire -= Time.deltaTime;
        if (firing && currentAmmo > 0 && currentTimeToFire <= 0)
        {
            //Debug.Log("FIRE");
            currentTimeToFire = fireRate;
            currentResetTime = resetTime;
            weaponAnim.Play();
            audioSource.PlayOneShot(fireSound);

            RaycastHit hit;
            Physics.Raycast(firePoint.position, player.position - firePoint.position, out hit);
            float accuracymultiplier = (float)heat / weaponData.maxHeat;

            for (int i = 0; i < numShots; i++)
            {
                var angle = Quaternion.AngleAxis(Random.Range(-accuracy * accuracymultiplier, accuracy * accuracymultiplier), new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
                Physics.Raycast(firePoint.position, (hit.point - firePoint.position).normalized, out hit);
                if (hit.collider.gameObject.layer == 7)
                {
                    Instantiate(decal, hit.point, Quaternion.identity);
                }
                else if (hit.collider.gameObject.layer == 3)
                {
                    playerHealth.TakeDamage(damage,transform.position);
                }
            }

            heat++;
            currentAmmo--;
        }
        else
        {
            currentResetTime -= Time.deltaTime;
            if (currentResetTime <= 0)
                heat = baseHeat;
        }
    }
}
