using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Gun : MonoBehaviour
{
    private Transform MainCam;
    public Transform firepoint;
    public GameObject muzzleflash;
    public PlayerInput playerInput;
    private TMP_Text txtHeat, txtAmmo, txtReload;
    public float damage = 10, range = 100;

    private bool firing = false;
    private bool readyToFire = false;
    
    [SerializeField]private float rateoffire = 0.1f, resetTime, accuracy, maxHeat, reloadTime;
    [SerializeField] private int clipSize, reserveSize;
    [SerializeField] private AudioSource sound;
    [SerializeField] private AudioClip firesound;
    [SerializeField] private LayerMask ignorePlayer;
    [SerializeField] private GameObject decal;
    private WaitForSeconds firedelay;
    private float heat;
    private float currentResetTime;
    private int currentAmmo, currentReserve;

    private InputAction fire, aim, reload;


    private Vector3 axis = new Vector3(1, 1, 0);

    private void Start()
    {
        playerInput = GameObject.Find("PlayerV5").GetComponent<PlayerInput>();
        fire = playerInput.actions["Attack"];
        aim = playerInput.actions["Aim"];
        reload = playerInput.actions["Reload"];
        txtHeat = GameObject.Find("txtHeat").GetComponent<TMP_Text>();
        txtAmmo = GameObject.Find("txtAmmo").GetComponent<TMP_Text>();
        txtReload = GameObject.Find("txtReload").GetComponent<TMP_Text>();
        currentAmmo = clipSize;
        currentReserve = reserveSize;
    }
    private void OnEnable()
    {
        firedelay = new WaitForSeconds(rateoffire);
        MainCam = Camera.main.transform;
        StartCoroutine(Autofire());
        
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
    public void Fire()
    {
        

    }

    
    private IEnumerator Autofire()
    {
        while (true)
        {
            muzzleflash.SetActive(firing);
            if (firing && readyToFire)
            {
                currentResetTime = resetTime;
                sound.PlayOneShot(firesound);
                RaycastHit hit;
                if (Physics.Raycast(MainCam.position, MainCam.forward, out hit, range, ignorePlayer))
                {
                    float accuracymultiplier = heat / maxHeat;
                    var angle = Quaternion.AngleAxis(Random.Range(-accuracy * accuracymultiplier,accuracy * accuracymultiplier), new Vector3(Random.Range(-1f,1f),Random.Range(-1f,1f),Random.Range(-1f,1f)));
                    if (Physics.Raycast(firepoint.position,angle * (hit.point - firepoint.position).normalized , out hit, ignorePlayer))
                    {
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
                    heat++;
                    currentAmmo--;
                    heat = Mathf.Clamp(heat, 0, maxHeat);
                }
                yield return firedelay;
            }
            else
            {
                currentResetTime -= Time.deltaTime;
                if (currentResetTime <= 0)
                    heat = 0;
                
                yield return null;
            }
            txtHeat.text = $"Heat: {heat}/{maxHeat}";
        }
        
    }

    private IEnumerator Reload()
    {
        muzzleflash.SetActive(false);
        yield return new WaitForSeconds(reloadTime);
        StartCoroutine(Autofire());
        txtReload.enabled = false;
    }

    private void Update()
    {
        firing = fire.ReadValue<float>() > 0 && aim.ReadValue<float>() > 0 && currentAmmo > 0;
        readyToFire = true;
        txtAmmo.text = $"Ammo: {currentAmmo}/{currentReserve}";
        if ((reload.triggered && currentAmmo < clipSize && currentReserve > 0) || (firing && currentAmmo == 0) )
        {
            StopAllCoroutines();
            txtReload.enabled = true;
            StartCoroutine(Reload());
            int amountneeded = clipSize - currentAmmo;
            if (amountneeded >= currentReserve)
            {
                currentAmmo += currentReserve;
                currentReserve = 0;
            }
            else
            {
                currentAmmo = clipSize;
                currentReserve -= amountneeded;
            }
        }
    }

    
}

