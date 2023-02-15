using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    //Script attached to the player for controlling the ghost.
    [HideInInspector]public GameObject ghostPrefab;
    public GameObject ghost;
    public Transform gTransform;
    public Transform gAttach;
    public Transform gAttachIdle;
    public Transform gAttachAttack;
    public Transform gAttachBlock;
    [SerializeField] private SkinnedMeshRenderer gMaterial;
    public Transform grabbedObject;
    public Transform lookTarget;
    public Grabbable grabbedObjectGrabbable;
    [SerializeField] private Transform grabPoint;
    private new Transform camera;
    private Animator animator;
    private GhostAnimator ghostAnimator;
    public PlayerController playerController;
    public EnemyStateController targetEnemy;
    public float ghostActiveTimer = 0;
    private float timeSinceLastAttack = 5;
    public bool preview = false;
    public bool takedown = false;
    private int attackCounter = 0;

    private int animPunch1;
    private int animPunch2;
    private int animKick;
    private int animTakedown;
    private int animGuard;
    private static readonly int hashBlocking = Animator.StringToHash("Blocking");

    public bool summoned = true;

    //private void Start()
    //{
    //    camera = Camera.main.transform;
    //    ghost = Instantiate(ghostPrefab,transform.parent);
    //    gTransform = ghost.transform;
    //    gMaterial = ghost.GetComponentInChildren<SkinnedMeshRenderer>();
    //    animator = ghost.GetComponent<Animator>();
    //    ghostAnimator = ghost.GetComponent<GhostAnimator>();
    //    playerController.ghostAnimator = ghostAnimator;
    //    ghostAnimator.controller = this;
    //    ghostAnimator.playerController = playerController;
    //    ghostAnimator.playerHealth = GetComponent<PlayerHealth>();
    //    ghostAnimator.gAttach = gAttach;
    //    ghostAnimator.gAttachAttack = gAttachAttack;
    //    ghostAnimator.gAttachBlock = gAttachBlock;
    //    ghostAnimator.gAttachIdle = gAttachIdle;

    //    animPunch1 = Animator.StringToHash("Punch1");
    //    animPunch2 = Animator.StringToHash("Punch2");
    //    animKick = Animator.StringToHash("Kick");
    //    animTakedown = Animator.StringToHash("Takedown");
    //    animGuard = Animator.StringToHash("Guard");
    //    gAttach = gAttachAttack;
    //}

    private void Update()
    {
        /*
        var attachpos = transform.position + (gameObject.transform.position - new Vector3(camera.transform.position.x, gameObject.transform.position.y, camera.transform.position.z)).normalized;
        gAttachAttack.position = new Vector3(attachpos.x, gAttachAttack.position.y, attachpos.z);
        float alpha = .5f;
        
        if (ghostActiveTimer > 0)
        {//If ghost is active make it look in direction of attack and set it to half transparent
            ghostActiveTimer -= Time.deltaTime;
            gTransform.position = Vector3.Lerp(gTransform.position, ghostAnimator.occupied ? gAttachAttack.position : gAttachIdle.position, Time.deltaTime * 10);
            var lookvector = gTransform.position + (playerController.transform.position - camera.position).normalized;
            lookvector.y = gTransform.position.y;
            gTransform.LookAt(lookvector);
        }
        else if (ghostAnimator.guard)
        {
            alpha = 1;
            gTransform.SetPositionAndRotation(Vector3.Lerp(gTransform.position, gAttachBlock.position, Time.deltaTime * 20), Quaternion.LookRotation(playerController.transform.forward));
        }
        else if (preview)
        {//If preview is move ghost to behind enemy and make it face enemy
            gTransform.SetPositionAndRotation(gAttach.position, Quaternion.LookRotation(new Vector3(lookTarget.position.x, gTransform.position.y, lookTarget.position.z) - gTransform.position));
            alpha = 0.5f;
        }
        else if (takedown)
        {//If playing takedown animation move ghost to behind enemy and make it face enemy
            gTransform.SetPositionAndRotation(gAttach.position, Quaternion.LookRotation(new Vector3(targetEnemy.transform.position.x, gTransform.position.y, targetEnemy.transform.position.z) - gTransform.position));
            alpha = 0.8f;
        }
        else if (summoned)
        {
            gTransform.SetPositionAndRotation(Vector3.Lerp(gTransform.position, gAttachIdle.position, Time.deltaTime * 30), Quaternion.Lerp(gTransform.rotation, playerController.transform.rotation, Time.deltaTime * 10));
            alpha = 0.5f;
        }
        else
        {
            alpha = 0;
            gTransform.SetPositionAndRotation(Vector3.Lerp(gTransform.position,  gAttachIdle.position, Time.deltaTime * 10), Quaternion.Lerp(gTransform.rotation, playerController.transform.rotation, Time.deltaTime * 10));
        }
        //Set Alpha Transparency Of Ghost To Make It Fade In and Out
        gMaterial.material.color = new Color(gMaterial.material.color.r, gMaterial.material.color.g, gMaterial.material.color.b, Mathf.Lerp(gMaterial.material.color.a,ghostAnimator.occupied ? 1 : alpha, Time.deltaTime * 10f));

        timeSinceLastAttack += Time.deltaTime;

        animator.SetBool(hashBlocking,ghostAnimator.guard);
        if (timeSinceLastAttack > 1.25f)//Reset Attack Counter if Enough Time Has Passed
        {
            attackCounter = 0;
        }
        */
    }


    public void SetTarget(Transform target, Transform attach)
    {
        preview = true;
        gAttach = attach;
        lookTarget = target;
    }
    public void Guard()
    {
        animator.Play(animGuard);
    }

    public void EndTarget()
    {
        preview = false;
        gAttach = gAttachAttack;
    }


    public void Attack()//Attack + Attack + Pause + Attack : Kick
    {
        /*
        if (!ghostAnimator.occupied && !ghostAnimator.guard)
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
        */
    }

    //public void Takedown(EnemyStateController targetEnemy)
    //{//Play longer punch animation
    //    this.targetEnemy = targetEnemy;
    //    animator.Play(animTakedown);
    //    takedown = true;
    //    ghostAnimator.occupied = true;
    //    preview = false;
    //    gAttach = targetEnemy.gAttach;
    //    //ghostActiveTimer = 1.5f;
    //}
}
