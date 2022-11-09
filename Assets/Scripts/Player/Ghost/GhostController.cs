using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    //Script attached to the player for controlling the ghost.
    [SerializeField] private GameObject ghostPrefab;
    public GameObject ghost;
    public Transform gTransform;
    public Transform gAttach;
    public Transform gAttachAttack;
    [SerializeField] private SkinnedMeshRenderer gMaterial;
    public Transform grabbedObject;
    public Transform lookTarget;
    public Grabbable grabbedObjectGrabbable;
    [SerializeField] private Transform grabPoint;
    private new Transform camera;
    private Animator animator;
    public PlayerController playerController;
    public EnemyStateController targetEnemy;
    public float ghostActiveTimer = 0;
    private float timeSinceLastAttack = 5;
    private float desiredAlpha;
    public bool summoned = false;
    public bool preview = false;
    public bool takedown = false;
    private int attackCounter = 0;

    private int animPunch1;
    private int animPunch2;
    private int animKick;
    private int animTakedown;
    private int animGuard;

    public bool occupied = false;
    public bool grabbing = false;

    private void Start()
    {
        camera = Camera.main.transform;
        ghost = Instantiate(ghostPrefab,this.transform.parent);
        gTransform = ghost.transform;
        gMaterial = ghost.GetComponentInChildren<SkinnedMeshRenderer>();
        animator = ghost.GetComponent<Animator>();
        ghost.GetComponent<GhostAnimator>().controller = this;
        animPunch1 = Animator.StringToHash("Punch1");
        animPunch2 = Animator.StringToHash("Punch2");
        animKick = Animator.StringToHash("Kick");
        animTakedown = Animator.StringToHash("Takedown");
        animGuard = Animator.StringToHash("Guard");
        gAttach = gAttachAttack;
    }

    private void Update()
    {
        var occupied = grabbing || this.occupied;
        var attachpos = transform.position + (gameObject.transform.position - new Vector3(camera.transform.position.x, gameObject.transform.position.y, camera.transform.position.z)).normalized;
        gAttachAttack.position = new Vector3(attachpos.x, gAttachAttack.position.y, attachpos.z);
        float alpha;

        if (ghostActiveTimer > 0)
        {//If ghost is active make it look in direction of attack and set it to half transparent
            ghostActiveTimer -= Time.deltaTime;
            gTransform.position = Vector3.Lerp(gTransform.position, playerController.isAiming ? gAttach.position : gAttachAttack.position, Time.deltaTime * 10);

            var lookvector = gTransform.position + (gTransform.position - gameObject.transform.position).normalized;
            lookvector.y = gTransform.position.y;
            gTransform.LookAt(lookvector);
            alpha = 0.5f;
        }
        else if(preview)
        {//If preview is move ghost to behind enemy and make it face enemy
            gTransform.position = gAttach.position;
            gTransform.rotation = Quaternion.LookRotation(new Vector3(lookTarget.position.x, gTransform.position.y, lookTarget.position.z) - gTransform.position);
            alpha = 0.5f;
        }
        else if (takedown)
        {//If playing takedown animation move ghost to behind enemy and make it face enemy
            gTransform.position = gAttach.position;
            gTransform.rotation = Quaternion.LookRotation(new Vector3(targetEnemy.transform.position.x, gTransform.position.y, targetEnemy.transform.position.z) - gTransform.position);
            alpha = 0.8f;
        }
        else
        {
            alpha = desiredAlpha;
            gTransform.position = gAttachAttack.position;
            Hide();
        }
        //Set Alpha Transparency Of Ghost To Make It Fade In and Out
        gMaterial.material.color = new Color(gMaterial.material.color.r, gMaterial.material.color.g, gMaterial.material.color.b, Mathf.Lerp(gMaterial.material.color.a,occupied ? 1 : alpha, Time.deltaTime * 10f));
        


        timeSinceLastAttack += Time.deltaTime;
        if (timeSinceLastAttack > 1.25f)//Reset Attack Counter if Enough Time Has Passed
        {
            attackCounter = 0;
        }
        if (grabbedObject != null)
        {
            grabbedObject.transform.position = grabPoint.position;
            grabbedObject.transform.rotation = grabPoint.rotation;
        }
    }

    //Not used in final game as there are not grabbable objects
    public void Grab()
    {
        if (!grabbing)
        {
            grabbedObject = playerController.interactable.gameObject.transform;
            grabbing = true;
            grabbedObjectGrabbable = grabbedObject.GetComponent<Grabbable>();
            grabbedObjectGrabbable.Grab();
        }
        else
        {
            grabbedObjectGrabbable.UnGrab();
            grabbedObject = null;
            grabbedObjectGrabbable = null;
            grabbing = false;
        }
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

    private void Hide()
    {
        desiredAlpha = 0;
        
    }

    private void Show()
    {
        desiredAlpha = 0.8f;
    }

    public void Attack()//Attack + Attack + Pause + Attack : Kick
    {
        if (!occupied)
        {
            Show();
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

    public void Takedown(EnemyStateController targetEnemy)
    {//Play longer punch animation
        Show();
        this.targetEnemy = targetEnemy;
        animator.Play(animTakedown);
        takedown = true;
        occupied = true;
        preview = false;
        gAttach = targetEnemy.gAttach;
        //ghostActiveTimer = 1.5f;
    }
}
