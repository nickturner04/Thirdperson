using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GhostAnimator : MonoBehaviour
{
    [Header("General Properties")]
    //Control Variables
    public Transform gAttach;
    public Transform gAttachIdle;
    public Transform gAttachAttack;
    public Transform gAttachBlock;
    private new Transform camera;

    public PlayerController playerController;
    public PlayerHealth playerHealth;

    public bool occupied = false;
    public bool summoned = false;

    public float ghostActiveTimer = 0;
    public float timeSinceLastAttack = 5;
    public int attackCounter = 0;

    //Animation Variables
    [HideInInspector]public Animator animator;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private GameObject[] hitboxes;
    private SkinnedMeshRenderer gMaterial;

    [SerializeField] private int punchDamage;
    [SerializeField] private int kickDamage;

    private readonly int animPunch1 = Animator.StringToHash("Punch1");
    private readonly int animPunch2 = Animator.StringToHash("Punch2");
    private int animKick = Animator.StringToHash("Kick");
    private readonly int animTakedown = Animator.StringToHash("Takedown");
    private readonly int animGuard = Animator.StringToHash("Guard");
    private readonly int hashBlocking = Animator.StringToHash("Blocking");

    protected AudioSource audioSource;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private ParticleSystem appearParticle;
    [SerializeField] private ParticleSystem[] auraParticles;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        gMaterial = GetComponentInChildren<SkinnedMeshRenderer>();
        camera = Camera.main.transform;
    }

    private void Update()
    {
        var attachpos = playerController.transform.position + (playerController.transform.position - new Vector3(camera.position.x, playerController.transform.position.y, camera.position.z)).normalized;
        gAttachAttack.position = new Vector3(attachpos.x, gAttachAttack.position.y, attachpos.z);
        float alpha = .5f;

        if (ghostActiveTimer > 0)
        {//If ghost is active make it look in direction of attack and set it to half transparent
            ghostActiveTimer -= Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, occupied ? gAttachAttack.position : gAttachIdle.position, Time.deltaTime * 10);
            var lookvector = transform.position + (playerController.transform.position - camera.position).normalized;
            lookvector.y = transform.position.y;
            transform.LookAt(lookvector);
        }
        else if (playerController.guard)
        {
            alpha = 1;
            transform.SetPositionAndRotation(Vector3.Lerp(transform.position, gAttachBlock.position, Time.deltaTime * 20), Quaternion.LookRotation(playerController.transform.forward));
            playerHealth.TakeEnergyDamage(20 * Time.deltaTime);
        }
 
        else if (summoned)
        {
            transform.SetPositionAndRotation(Vector3.Lerp(transform.position, gAttachIdle.position, Time.deltaTime * 30), Quaternion.Lerp(transform.rotation, playerController.transform.rotation, Time.deltaTime * 10));
            alpha = 0.5f;
        }
        else
        {
            alpha = 0;
            transform.SetPositionAndRotation(Vector3.Lerp(transform.position, gAttachIdle.position, Time.deltaTime * 10), Quaternion.Lerp(transform.rotation, playerController.transform.rotation, Time.deltaTime * 10));
        }
        //Set Alpha Transparency Of Ghost To Make It Fade In and Out
        gMaterial.material.color = new Color(gMaterial.material.color.r, gMaterial.material.color.g, gMaterial.material.color.b, Mathf.Lerp(gMaterial.material.color.a, occupied ? 1 : alpha, Time.deltaTime * 10f));

        timeSinceLastAttack += Time.deltaTime;

        animator.SetBool(hashBlocking, playerController.guard);
        if (timeSinceLastAttack > 1.25f)//Reset Attack Counter if Enough Time Has Passed
        {
            attackCounter = 0;
        }
    }

    public void Appear()
    {
        summoned = true;
        appearParticle.Play();
        foreach (var item in auraParticles)
        {
            item.Play();
        }
    }
    public virtual void Disappear()
    {
        summoned = false;
        appearParticle.Stop();
        foreach (var item in auraParticles)
        {
            item.Stop();
        }
    }

    public void Attack()//Attack + Attack + Pause + Attack : Kick
    {
        if (!occupied && !playerController.guard)
        {
            ghostActiveTimer = 1.5f;
            switch (attackCounter)
            {//Play a different punch animation depending on how many times the player has attacked
                case 0:
                    animator.Play(animPunch1);
                    attackCounter = 1;
                    timeSinceLastAttack = 0;
                    break;
                case 1:
                    animator.Play(animPunch2);
                    attackCounter = 2;
                    timeSinceLastAttack = 0;
                    break;
                case 2:
                    if (timeSinceLastAttack < 0.75f)
                    {
                        animator.Play(animPunch1);
                        attackCounter = 1;
                        timeSinceLastAttack = 0;
                    }
                    else
                    {
                        animator.Play(animKick);
                        attackCounter = 0;
                        timeSinceLastAttack = 0;
                    }
                    break;
                default:
                    break;
            }
        }
    }
    public virtual void AbilityL1Start(InputAction.CallbackContext context)
    {
        Debug.Log("Began Charging L1 Ability");
        
    }

    public virtual void AbilityL1Stop(InputAction.CallbackContext context)
    {
        Debug.Log($"Stopped Charging L1 Ability after {context.duration}");
        
    }

    public virtual void AbilityL2(InputAction.CallbackContext _)
    {
        Debug.Log("ability L2 activated");
    }

    public void StartAttack()
    {
        occupied = true;
    }

    public void EndAttack()
    {
        foreach (var item in hitboxes)
        {
            item.SetActive(false);
        }
        occupied = false;
    }

    public void SpawnPunch1()
    {
        hitboxes[0].SetActive(true);
        
    }
    public void SpawnPunch2()
    {
        hitboxes[1].SetActive(true);
    }
    public void SpawnKick()
    {
        hitboxes[2].SetActive(true);
    }
}
