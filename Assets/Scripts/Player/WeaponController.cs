using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Inventory))]
public class WeaponController : MonoBehaviour
{
    private Transform MainCam;
    private LineRenderer laserSight;
    private Animator animator;
    private Transform firepoint;
    private ParticleSystem muzzleFlash;
    private GameObject weaponObject;
    private WeaponAnim weaponAnim;

    [SerializeField] private LayerMask ignorePlayer;
    [SerializeField] private GameObject decal;
    [SerializeField] private Transform attachPoint;
    [SerializeField] private WeaponData[] weapons;
    [SerializeField] private Inventory inventory;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Transform aimTarget;
    [SerializeField] private LabelManager labelManager;
    public WeaponData weaponData;
    

    public int currentAmmo, currentReserve, clipSize, baseHeat;
    private bool firing, automatic,fired = false;
    public bool aiming = false, reloading = false;
    private int maxHeat, heat, reserveSize, numShots;
    private float fireRate, resetTime, accuracy, damage;
    private AudioClip fireSound;

    RaycastHit hit;

    private float currentTimeToFire, currentResetTime = 0;

    private void Start()
    {
        laserSight = GetComponent<LineRenderer>();
        MainCam = Camera.main.transform;
        //Equip(weaponData);
    }

    public void Equip(WeaponData weapon)
    {
        weaponData = weapon;
        MainCam = Camera.main.transform;
        maxHeat = weapon.maxHeat;
        fireRate = weapon.fireRate;
        resetTime = weapon.resetTime;
        accuracy = weapon.accuracy;
        damage = weapon.damage;
        clipSize = weapon.clipSize;
        numShots = weapon.numShots;
        baseHeat = weapon.baseHeat;
        automatic = weapon.automatic;
        Destroy(weaponObject);
        weaponObject = Instantiate(weapon.weaponModel, attachPoint);
        weaponAnim = weaponObject.GetComponent<WeaponAnim>();
        firepoint = weaponAnim.muzzleFlash.transform;
        muzzleFlash = firepoint.gameObject.GetComponent<ParticleSystem>();
        fireSound = weapon.firesound;
        currentTimeToFire = 0;

        labelManager.SetWeapon(weapon.displaySprite, weapon.displayName);
    }

    public void Fire(bool firing)
    {
        this.firing = firing;
    }

    public void AltFire()
    {

    }

    public void Aim()
    {

    }

    public void Reload()
    {
        reloading = true;
        int amountneeded = clipSize - inventory.ammo[1, (int)weaponData.ammoType];
        if (amountneeded >= inventory.ammo[0, (int)weaponData.ammoType])
        {
            inventory.ammo[1, (int)weaponData.ammoType] += inventory.ammo[0, (int)weaponData.ammoType];
            inventory.ammo[0, (int)weaponData.ammoType] = 0;
        }
        else
        {
            inventory.ammo[1, (int)weaponData.ammoType] = clipSize;
            inventory.ammo[0, (int)weaponData.ammoType] -= amountneeded;
        }
        
    }

    private void Update()
    {
        var ammo = inventory.ammo[1, (int)weaponData.ammoType];
        var reserve = inventory.ammo[0, (int)weaponData.ammoType];
        currentTimeToFire -= Time.deltaTime;
        
        if (aiming && !reloading)
        {
            Physics.Raycast(firepoint.position, MainCam.forward, out hit, Mathf.Infinity, ignorePlayer);
            //crosshair.position = Camera.main.WorldToScreenPoint(hit.point);
            laserSight.SetPosition(0, firepoint.position);
            laserSight.SetPosition(1, hit.point);
            laserSight.enabled = true;
            if (firing && ammo > 0 && currentTimeToFire <= 0)
            {
                currentResetTime = resetTime;
                currentTimeToFire = fireRate;
                weaponAnim.Play();
                audioSource.PlayOneShot(fireSound);




                //Debug.Log("Camera Hit");
                float accuracymultiplier = (float)heat / weaponData.maxHeat;
                //Debug.Log(accuracymultiplier);
                for (int i = 0; i < numShots; i++)
                {
                    var angle = Quaternion.AngleAxis(Random.Range(-accuracy * accuracymultiplier, accuracy * accuracymultiplier), new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
                    if (Physics.Raycast(firepoint.position, angle * (hit.point - firepoint.position).normalized, out hit, ignorePlayer))
                    {

                        //Debug.Log("Weapon Hit");
                        if (hit.collider.gameObject.layer == 7)
                        {
                            Instantiate(decal, hit.point, Quaternion.identity);
                        }
                        if (hit.collider.TryGetComponent(out Health health))
                        {
                            health.TakeDamage(damage);
                        }
                        if (hit.collider.TryGetComponent(out Rigidbody rb))
                        {
                            Vector3 dir = (rb.transform.position - hit.point).normalized;
                            rb.AddForce(dir * 100);
                        }
                    }
                }


                heat++;
                inventory.ammo[1, (int)weaponData.ammoType]--;


                if (heat > maxHeat)
                    heat = maxHeat;
            }
            else
            {
                currentResetTime -= Time.deltaTime;
                if (currentResetTime <= 0)
                    heat = baseHeat;
            }
            
        }
        else
        {
            laserSight.enabled = false;
        }
        
        currentAmmo = ammo;
        currentReserve = reserve;

        labelManager.SetAmmo(currentAmmo,currentReserve);
    }

        
}
