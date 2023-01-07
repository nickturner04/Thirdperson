using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Inventory))]
public class WeaponController : MonoBehaviour
{
    private Transform MainCam;
    private LineRenderer laserSight;
    private Transform firepoint;
    private GameObject weaponObject;
    private WeaponAnim weaponAnim;

    [SerializeField] private LayerMask ignorePlayer;
    [SerializeField] private GameObject decal;
    public Transform attachPoint;
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
    {//Copy data from the equipped weapon to the local variables
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
        fireSound = weapon.firesound;
        currentTimeToFire = 0;

        labelManager.SetWeapon(weapon.displaySprite, weapon.displayName);
        labelManager.UpdateAmmo(UnityEngine.UIElements.DisplayStyle.Flex);
    }

    public void Unequip()
    {
        labelManager.SetWeapon(null, "None");
        labelManager.UpdateAmmo(UnityEngine.UIElements.DisplayStyle.None);
        Destroy(weaponObject);
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
        int amountneeded = clipSize - inventory.ammo[1, weaponData.ammoType];
        if (amountneeded >= inventory.ammo[0, weaponData.ammoType])
        {
            inventory.ammo[1, weaponData.ammoType] += inventory.ammo[0, weaponData.ammoType];
            inventory.ammo[0, weaponData.ammoType] = 0;
        }
        else
        {
            inventory.ammo[1, weaponData.ammoType] = clipSize;
            inventory.ammo[0, weaponData.ammoType] -= amountneeded;
        }
        
    }

    private void Update()
    {
        var ammo = inventory.ammo[1, weaponData.ammoType];
        var reserve = inventory.ammo[0, weaponData.ammoType];
        currentTimeToFire -= Time.deltaTime;
        
        if (aiming && !reloading)
        {
            //Send a raycast from the end of the gun in the direction the camera faces
            Physics.Raycast(firepoint.position, MainCam.forward, out hit, Mathf.Infinity, ignorePlayer);
            //crosshair.position = Camera.main.WorldToScreenPoint(hit.point);
            //Draw Laser Sight
            laserSight.SetPosition(0, firepoint.position);
            laserSight.SetPosition(1, hit.point);
            laserSight.enabled = true;
            //Check if there is ammo left and the gun is ready to fire
            if (firing && ammo > 0 && currentTimeToFire <= 0)
            {
                //Reset the time on the spread
                currentResetTime = resetTime;
                currentTimeToFire = fireRate;
                weaponAnim.Play();
                audioSource.PlayOneShot(fireSound);


                //Debug.Log("Camera Hit");
                float accuracymultiplier = (float)heat / weaponData.maxHeat;
                //heat is a measure of how many bullets have been fired, it makes firing in short bursts more accurate
                //Debug.Log(accuracymultiplier);
                for (int i = 0; i < numShots; i++)
                {
                    //Add an element of randomness to each shot depending on how many shots have already been fired
                    var angle = Quaternion.AngleAxis(Random.Range(-accuracy * accuracymultiplier, accuracy * accuracymultiplier), new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
                    
                    if (Physics.Raycast(firepoint.position, angle * (hit.point - firepoint.position).normalized, out hit, ignorePlayer))
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


                heat++;
                inventory.ammo[1, weaponData.ammoType]--;


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
