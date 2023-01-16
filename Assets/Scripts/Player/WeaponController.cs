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
        Debug.Log(~(1 << 3));
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
        int amountneeded = clipSize - inventory.currentAmmo[weaponData.slot];
        if (amountneeded >= inventory.reserveAmmo[weaponData.ammoType])
        {
            inventory.currentAmmo[weaponData.slot] += inventory.reserveAmmo[weaponData.ammoType];
            inventory.reserveAmmo[weaponData.ammoType] = 0;
        }
        else
        {
            inventory.currentAmmo[weaponData.slot] = clipSize;
            inventory.reserveAmmo[weaponData.ammoType] -= amountneeded;
        }
        
    }

    private void Update()
    {
        var ammo = inventory.currentAmmo[weaponData.slot];
        var reserve = inventory.reserveAmmo[weaponData.ammoType];
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
                
                weaponData.Fire(firepoint.position, (hit.point - firepoint.position).normalized,accuracymultiplier);
                
                heat++;
                inventory.currentAmmo[weaponData.slot]--;


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

        labelManager.SetAmmo(ammo,reserve);
    }

        
}
