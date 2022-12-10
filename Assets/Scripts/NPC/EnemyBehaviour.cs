using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using UnityEngine.Animations.Rigging;

[System.Serializable]
public struct Interrupt
{
    public Interrupt(int priority, Vector3 position)
    {
        this.priority = priority;
        this.position = position;
    }

    public int priority;
    public Vector3 position;
}

[RequireComponent(typeof(Sight), typeof(NavMeshAgent))]
public class EnemyBehaviour : MonoBehaviour
{
    enum ActionState { Idle, Working }
    ActionState state;

    public BehaviourTree tree;
    public BehaviourTree treeNormal = new("Normal");
    public BehaviourTree treeAlert = new("Alert");
    private bool lookExecuting = false;
    private bool goExecuting = false;

    private Transform player;
    private Transform hostageArea;
    [SerializeField] private Transform feet;
    [SerializeField] private Transform target;
    private Sight sight;
    [HideInInspector]public NavMeshAgent agent;
    private EnemyStateController stateController;
    private Animator animator;
    public EnemyWeaponController weaponController;

    [HideInInspector]
    public EnemyManager enemyManager;
    [HideInInspector]
    public Vector3[] patrolPoints;
    public int nextPoint = 0;
    public Vector3 alertDestination = Vector3.zero;
    private bool alertMoving = false;
    private LayerMask defaultMask;
    private bool FoundDestination = false;
    public bool surround = true;

    //Interrupt
    public Interrupt interrupt = new(0, Vector3.zero);

    //Timers
    private float waitTime;
    private float sightTime;
    private float lookTime = 0;
    private float currentAttackTime = 0;
    private float currentAttackCooldown = 0;
    private bool reloading = false;
    int rlAnim;
    [SerializeField] private float attackTime = 0.35f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private TMP_Text sightText;
    [SerializeField] private TMP_Text visibilityText;
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip alert;
    [SerializeField] private Transform aimTarget;
    [SerializeField] private Rig rig;

    private int speedHash;

    private void OnDisable()
    {
       
       weaponController.Fire(false);
        
    }
    private void Start()
    {
        speedHash = Animator.StringToHash("SPEED");
        player = GameObject.Find("PlayerV5").transform;
        hostageArea = GameObject.Find("HostageArea").transform;
        sight = GetComponent<Sight>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        weaponController = GetComponent<EnemyWeaponController>();
        stateController = GetComponent<EnemyStateController>();
        rlAnim = Animator.StringToHash("RELOADING");
        defaultMask = LayerMask.GetMask("Terrain");
        target.parent = null;
        //Set Normal Tree
        Parallel parallel = new("Base");
        Selector selector = new("Selector");
        Sequence checkSight = new("Check Sight");
        Leaf fillSight = new("Fill Sight", FillSight);
        Leaf callHelp = new("Call Help", CallHelp);
        Decorator isInterrupt = new("Is There An Interrupt?", IsInterrupt);
        Selector checkInterrupt = new("Check Interrupt");
        Sequence lookAtInterrupt = new("Look At Interrupt");
        Leaf turnToInterrupt = new("Turn To Interrupt", TurnToInterrupt);
        Leaf goToInterrupt = new("Go To Interrupt", GoToInterrupt);
        Selector patrol = new("Patrol");
        Leaf wait = new("Wait", Wait);
        Leaf goToPatrolPoint = new("Go To Patrol Point", GoToPatrolPoint);
        treeNormal.AddChild(parallel);
        parallel.AddChild(fillSight);
        parallel.AddChild(selector);
        selector.AddChild(isInterrupt);
        isInterrupt.AddChild(checkInterrupt);
        checkInterrupt.AddChild(turnToInterrupt);
        checkInterrupt.AddChild(goToInterrupt);
        checkInterrupt.AddChild(callHelp);
        selector.AddChild(patrol);
        patrol.AddChild(wait);
        patrol.AddChild(goToPatrolPoint);

        //Set Alert Tree
        //Sequence - Check Position, Go To Position, Attack, Reload
        Parallel alertBase = new("Alert Base");
        Leaf checkPosition = new("CheckPosition", CheckPosition);
        Leaf movetoposition = new("MoveToPosition",MoveToPosition);
        Sequence attack = new("Attack");
        Leaf shoot = new("Shoot", Shoot);
        Leaf reload = new("Reload", Reload);
        treeAlert.AddChild(alertBase);
        alertBase.AddChild(checkPosition);
        alertBase.AddChild(attack);
        attack.AddChild(movetoposition);
        attack.AddChild(shoot);
        attack.AddChild(reload);
        tree = treeNormal;
        //tree.PrintTree();
    }
    /*PLAN FOR ALERT TREE
     * Parallel
     * *Check Position
     * 
     * *Sequence
     * **Move
     * **Shoot
     * **Reload
     */
    /*
     * NORMAL BEHAVIOUR:
     * If interrupt's priority is zero:
     *  Move To Next Patrol Poin
     * Else:
     *  switch interrupt.priority
     *      1: Look at interrupt
     *      2: Walk To Interrupt
     *      3: Run to Interrupt
     *      4: Run faster to Interrupt
     *      5: Call Alert
     *
     *While this is running:
     *      if the player is visible, add an interrupt of priority 5
     */

    public Node.Status MoveToPosition()
    {
        if (alertMoving)
        {
            if (Vector3.Distance(transform.position, player.position) <= 5)
            {
                Debug.Log("CLOSE");
                FoundDestination = true;
                alertMoving = false;
                alertDestination = transform.position;
                agent.SetDestination(transform.position);
                animator.SetFloat(speedHash, 0f);
                return Node.Status.Success;
            }
            if (agent.remainingDistance <= 0)
            {
                alertMoving = false;
                agent.SetDestination(transform.position);
                animator.SetFloat(speedHash, 0f);
                return Node.Status.Success;
            }
            else
            {
                if (alertDestination != agent.destination)
                {
                    agent.SetDestination(alertDestination);
                }
                animator.SetFloat(speedHash, 1f);
                transform.LookAt(new Vector3(alertDestination.x, transform.position.y, alertDestination.z));
            }
        }
        else
        {
            
            if (Vector3.Distance(alertDestination,transform.position) < 3f)
            {
                //animator.SetFloat(speedHash, 0f);
                return Node.Status.Success;
            }
            else
            {
                agent.SetDestination(alertDestination);
                agent.speed = 6f;
                animator.SetFloat(speedHash, 1f);
                alertMoving = true;
                return Node.Status.Running;
            }
            
        }
        return Node.Status.Failure;
    }

    public Node.Status CheckPosition()
    {
        var alDes2 = new Vector3(alertDestination.x, alertDestination.y + 1, alertDestination.z);
        target.position = alDes2;
        if (sight.LineOfSight)
        {
            enemyManager.UpdatePlayerPosition(player.position);
        }
        //Check current position
        
        Debug.DrawRay(alDes2, player.position - alDes2);
        if (FoundDestination == false)
        {
            //Debug.Log("false");
            //Find New Position

            //Get a position within a circle of the current position
            for (int i = 0; i < 100; i++)
            {
                var thisPosition = GetRingPosition(transform.position, Random.Range(1f, Mathf.Ceil(i / 5))); ;
                if (thisPosition != Vector3.zero && !Physics.Raycast(thisPosition, player.position - thisPosition, Vector3.Distance(player.position, thisPosition), defaultMask))
                {
                    FoundDestination = true;
                    alertDestination = thisPosition;
                    goto SkipLoop;
                }
            }

            //Get a position within a circle of the player's position
            while (true)
            {
                var newPosition = GetRingPosition(surround ? player.position : transform.position, surround ? Random.Range(5f, 20f) : Random.Range(1f, 50f));
                if (newPosition != Vector3.zero && !Physics.Raycast(newPosition, player.position - newPosition, Vector3.Distance(player.position, newPosition), defaultMask))//If new position is 0,0,0 then try again to find a new one
                {
                    FoundDestination = true;
                    alertDestination = newPosition;
                    break;
                }
            }
        SkipLoop:;
            
        }
        else
        {
            var rc = Physics.Raycast(alDes2, (player.position - alDes2).normalized, Vector3.Distance(player.position,alDes2), defaultMask);
            if (rc)
            {
                FoundDestination = false;
            }
            else 
            {
                if (Vector3.Distance(transform.position, alDes2) <= 1)
                {
                    var pos = new Vector3(player.position.x, transform.position.y, player.position.z);
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(pos - transform.position), Time.deltaTime * 5);
                }
                //if (surround)
                //{
                //    NavMeshPath path = new();
                //    NavMesh.CalculatePath(alDes2, player.position, NavMesh.AllAreas, path);
                //    var dist = 0f;
                //    for (int i = 0; i < path.corners.Length - 1; i++)
                //    {
                //        dist += Vector3.Distance(path.corners[i], path.corners[i + 1]);
                //    }
                //    if (dist > 20f) FoundDestination = false;

                //}
                
            }
        }
        return Node.Status.Success;
    }

    //Get a position on the navmesh within a certain radius of a point, if the point is not on the navmesh then return zero
    private Vector3 GetRingPosition(Vector3 point, float range)
    {
        Vector3 randomPoint = point + Random.insideUnitSphere * range;

        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas)) return hit.position;
        return Vector3.zero;
    }

    public Node.Status GoToInterrupt()
    {
        if (goExecuting)
        {
            if (interrupt.priority > 4)
            {//If priority is 5 then end this task so that alert can be called
                goExecuting = false;
                return Node.Status.Success;
            }
            if (agent.remainingDistance <= 0)
            {
                if (lookTime > 0)
                {
                    animator.SetFloat(speedHash, 0f);
                    lookTime -= Time.deltaTime;
                    return Node.Status.Running;
                }
                else
                {
                    goExecuting = false;
                    interrupt.priority = 0;
                    return Node.Status.Success;
                }
            }
            else
            {
                return Node.Status.Running;
            }
        }
        else
        {
            agent.SetDestination(interrupt.position);
            if (Vector3.Distance(agent.pathEndPosition, interrupt.position) >= 2f)
            {
                return Node.Status.Success;
            }
            //Change Speed depending on priority
            if (interrupt.priority == 2)
            {
                agent.speed = 2;
                animator.SetFloat(speedHash, 0.3f);
            }
            else if (interrupt.priority == 3)
            {
                agent.speed = 3.5f;
                animator.SetFloat(speedHash, 0.4f);
            }
            else if (interrupt.priority == 4)
            {
                agent.speed = 4.5f;
                animator.SetFloat(speedHash, 0.5f);
            }
            else if (interrupt.priority >= 5)
            {
                return Node.Status.Failure;
            }

            goExecuting = true;
            lookTime = 5;

            return Node.Status.Running;
        }
    }

    public Node.Status TurnToInterrupt()
    {
        var pos = new Vector3(interrupt.position.x, transform.position.y, interrupt.position.z);
        var dir = (pos - transform.position).normalized;
        var angle = Vector3.Angle(transform.forward, dir);
        if (lookExecuting)
        {
            if (interrupt.priority != 1)
            {
                lookExecuting = false;
                rig.weight = 0;
                return Node.Status.Success;
            }
            else if (angle < 5)
            {
                if (lookTime > 0)
                {//Wait until timer is finished before returning to patrol
                    lookTime -= Time.deltaTime;
                    return Node.Status.Running;
                }
                else
                {
                    lookExecuting = false;
                    interrupt.priority = 0;
                    rig.weight = 0;
                    return Node.Status.Success;
                }
            }
            else
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 3);
                return Node.Status.Running;
            }
        }
        else
        {
            if (interrupt.priority == 1)
            {
                lookExecuting = true;
                lookTime = 5;
                rig.weight = 1;
                agent.SetDestination(transform.position);
                animator.SetFloat(speedHash, 0f);
                return Node.Status.Running;
            }
            else
            {
                lookExecuting = false;
                return Node.Status.Failure;
            }
        }
    }

    public bool IsInterrupt()
    {
        if (interrupt.priority == 0)
            return false;
        return true;

    }

    public Node.Status Shoot()
    {
        weaponController.Fire(false);
        if (weaponController.currentAmmo == 0)
        {
            reloading = true;
            return Node.Status.Success;
        }
        if (currentAttackTime <= 0)
        {
            currentAttackCooldown = attackCooldown + Random.Range(-1f, 1f);
            currentAttackTime = attackTime;
        }
        if (currentAttackCooldown > 0)
        {
            currentAttackCooldown -= Time.deltaTime;
            return Node.Status.Failure;
        }
        if (currentAttackTime > 0)
        {
            if (sight.visibility > 0) weaponController.Fire(true);
            else weaponController.Fire(false);

            currentAttackTime -= Time.deltaTime;
            return Node.Status.Failure;
        }
        return Node.Status.Failure;

    }

    //Stop movement and reload weapon
    public Node.Status Reload()
    {
        if (reloading && !animator.GetBool(rlAnim))
        {
            animator.SetBool(rlAnim, true);
            agent.isStopped = true;
            agent.SetDestination(transform.position);
            return Node.Status.Running;
        }
        else if (reloading && animator.GetBool(rlAnim))
        {
            return Node.Status.Running;
        }
        else
        {
            animator.SetBool(rlAnim, false);
            agent.isStopped = false;
            return Node.Status.Success;
        }

    }

    //This is called by the event at the end of the reload animation
    public void EndReload()
    {
        weaponController.currentAmmo = weaponController.clipSize;
        animator.SetBool(rlAnim, false);
        reloading = false;
    }

    //If enemy can see at least one body part from the player, create a level 5 interrupt
    public Node.Status FillSight()
    {
        if (sight.LineOfSight && !enemyManager.ignorePlayer)
        {
            AddInterrupt(5, player.position);
        }
        //Commented out as it didnt work
        /*
        else 
        {
            Debug.Log(sightTime);
            if (sight.visibility > 0.16f)
            {
                sightTime += Time.deltaTime;
            }
            else
            {
                sightTime = 0;
            }
            if (sightTime > 11)
            {
                AddInterrupt(3, player.position);
            }
            else if (sightTime > 22)
            {
                AddInterrupt(2, player.position);
            }
                
        }
        */
        return Node.Status.Failure;
    }

    public Node.Status CallHelp()
    {
        enemyManager.UpdatePlayerPosition(player.position);
        source.PlayOneShot(alert);
        return Node.Status.Success;
    }

    public Node.Status Wait()
    {
        if (waitTime > 0)
        {
            if (sight.visibility > 0) return Node.Status.Success;
            animator.SetFloat(speedHash, 0f);
            waitTime -= Time.deltaTime;
            return Node.Status.Running;
        }
        return Node.Status.Failure;
    }

    public Node.Status GoToPatrolPoint()
    {
        if (interrupt.priority != 0)
        {
            waitTime = 0;
            agent.SetDestination(transform.position);
            return Node.Status.Failure;
        }
        animator.SetFloat(speedHash, 0.4f);
        agent.speed = 3.5f;
        sightTime = 0;
        var s = GoToLocation(patrolPoints[nextPoint]);
        //Debug.Log(s);
        if (s == Node.Status.Running) return Node.Status.Running;
        else if (s == Node.Status.Success)
        {
            nextPoint++;
            if (nextPoint == patrolPoints.Length)
            {
                nextPoint = 0;
            }
            return Node.Status.Success;
        }
        return Node.Status.Failure;
    }

    public Node.Status GoToLocation(Vector3 destination)
    {
        if (interrupt.priority != 0)
        {
            waitTime = 0;
            agent.SetDestination(transform.position);
            state = ActionState.Idle;
            return Node.Status.Failure;
        }
        float dist = Vector3.Distance(destination, transform.position);
        if (state == ActionState.Idle)
        {
            agent.SetDestination(destination);
            state = ActionState.Working;
        }

        else if (Vector3.Distance(agent.pathEndPosition, destination) >= 2f)
        {
            state = ActionState.Idle;
            return Node.Status.Failure;
        }

        else if (dist < 2f)
        {
            state = ActionState.Idle;
            return Node.Status.Success;
        }
        return Node.Status.Running;
    }

    public void AddInterrupt(int priority, Vector3 position)
    {
        if (priority > interrupt.priority)
        {
            interrupt = new Interrupt(priority, position);
        }
    }

    public void CheckAwakeEnemies()
    {
        enemyManager.CheckAwakeEnemies();
    }

    private void Update()
    {
        aimTarget.position = lookExecuting ? interrupt.position : player.position;
        //If alert and can see player move head to look towards player
        rig.weight = (lookExecuting || (sight.LineOfSight && enemyManager.isAlert)) ? 1 : 0;
        
        if (stateController.state == EnemyStateController.EnemyState.Normal)
        {
            agent.isStopped = false;
            tree.Process();
        }
        else if (stateController.state == EnemyStateController.EnemyState.Stun)
        {
            sightTime = 0;
            agent.isStopped = true;
        }
        else if (stateController.state == EnemyStateController.EnemyState.Grab)//If enemy is grabbed by player, move to in front of player
        {
            transform.SetPositionAndRotation(hostageArea.transform.position, player.rotation);
        }
        else
        {
            agent.SetDestination(agent.transform.position);
            weaponController.Fire(false);
            rig.weight = 0;

            interrupt.priority = 0;
            if (reloading)
            {
                reloading = false;
                animator.SetBool(rlAnim, false);
            }
        }
        
        
    }
}
