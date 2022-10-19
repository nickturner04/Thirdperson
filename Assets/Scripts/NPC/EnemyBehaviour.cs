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
    Node.Status treeStatus = Node.Status.Running;

    public BehaviourTree tree;
    public BehaviourTree treeNormal = new BehaviourTree("Normal");
    public BehaviourTree treeAlert = new BehaviourTree("Alert");
    private bool lookExecuting = false;
    private Leaf goLeaf;

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
    private Vector3 alertDestination = Vector3.zero;
    private bool alertMoving = false;
    private LayerMask defaultMask;
    private bool FoundDestination = false;
    public bool surround = true;

    //Interrupt
    public Interrupt interrupt = new Interrupt(0, Vector3.zero);

    //Timers
    private float waitTime;
    private float sightTime;
    private float lookTime = 0;
    private float currentAttackTime = 0;
    private float currentAttackCooldown = 0;
    private bool reloading = false;
    int rlAnim;
    [SerializeField] private float attackTime = 0.25f;
    [SerializeField] private float attackCooldown = 3;
    [SerializeField] private TMP_Text sightText;
    [SerializeField] private TMP_Text visibilityText;
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip alert;
    [SerializeField] private Transform aimTarget;
    private Vector3 aimTargetFollow;
    [SerializeField] private Rig rig;

    private void OnDisable()
    {
       
       //weaponController.Fire(false);
        
    }
    private void OnDestroy()
    {
        Destroy(target.gameObject);
    }

    private void Start()
    {
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
        Parallel parallel = new Parallel("Base");
        Selector selector = new Selector("Selector");
        Sequence checkSight = new Sequence("Check Sight");
        Leaf fillSight = new Leaf("Fill Sight", FillSight);
        Leaf callHelp = new Leaf("Call Help", CallHelp);
        Decorator isInterrupt = new Decorator("Is There An Interrupt?", IsInterrupt);
        Selector checkInterrupt = new Selector("Check Interrupt");
        Sequence lookAtInterrupt = new Sequence("Look At Interrupt");
        Leaf turnToInterrupt = new Leaf("Turn To Interrupt", TurnToInterrupt);
        Leaf goToInterrupt = new Leaf("Go To Interrupt", GoToInterrupt);
        goLeaf = goToInterrupt;
        Selector patrol = new Selector("Patrol");
        Leaf wait = new Leaf("Wait", Wait);
        Leaf goToPatrolPoint = new Leaf("Go To Patrol Point", GoToPatrolPoint);
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
        Parallel alertBase = new Parallel("Alert Base");
        Leaf checkPosition = new Leaf("CheckPosition", CheckPosition);
        Leaf movetoposition = new Leaf("MoveToPosition",MoveToPosition);
        Sequence attack = new Sequence("Attack");
        Leaf chase = new Leaf("Chase", Chase);
        Leaf shoot = new Leaf("Shoot", Shoot);
        Leaf reload = new Leaf("Reload", Reload);
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
     * *Selector
     * **Move
     * **Shoot
     * **Reload
     */

    public Node.Status MoveToPosition()
    {
        if (alertMoving)
        {
            if (agent.remainingDistance < 0)
            {
                alertMoving = false;
                agent.SetDestination(transform.position);
                return Node.Status.Success;
            }
            else
            {
                if (alertDestination != agent.destination)
                {
                    agent.SetDestination(alertDestination);
                }
                transform.LookAt(new Vector3(alertDestination.x, transform.position.y, alertDestination.z));
            }
        }
        else
        {
            if (Vector3.Distance(alertDestination,transform.position) < 3f)
            {
                return Node.Status.Success;
            }
            else
            {
                agent.SetDestination(alertDestination);
                agent.speed = 6f;
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
            Debug.Log("false");
            //Find New Position
            var found = false;
            while (!found)
            {
                
                var newPosition = GetRingPosition(surround ? player.position : transform.position, surround ? Random.Range(5f, 20f) : Random.Range(1f, 50f));
                if (newPosition != Vector3.zero)
                {
                    found = true;
                    FoundDestination = true;
                    alertDestination = newPosition;
                }
            }
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
                if (surround)
                {
                    NavMeshPath path = new NavMeshPath();
                    NavMesh.CalculatePath(alDes2, player.position, NavMesh.AllAreas, path);
                    var dist = 0f;
                    if (path.corners.Length > 2)
                    {
                        for (int i = 0; i < path.corners.Length - 2; i++)
                        {
                            dist += Vector3.Distance(path.corners[i], path.corners[i + 1]);
                        }
                    }
                    else
                    {
                        dist = Vector3.Distance(alDes2, player.position);
                    }
                    if (dist > 20f) FoundDestination = false;

                }
                
            }
        }
        return Node.Status.Success;
    }

    private Vector3 GetRingPosition(Vector3 point, float range)
    {
        Vector3 randomPoint = point + Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas)) return hit.position;
        return Vector3.zero;
    }

    public Node.Status GoToInterrupt()
    {
        if (goLeaf.executing)
        {
            if (interrupt.priority > 4)
            {
                goLeaf.executing = false;
                return Node.Status.Success;
            }
            if (agent.remainingDistance <= 0)
            {
                if (lookTime > 0)
                {
                    lookTime -= Time.deltaTime;
                    return Node.Status.Running;
                }
                else
                {
                    goLeaf.executing = false;
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
            if (interrupt.priority == 2)
            {
                agent.speed = 2;
            }
            else if (interrupt.priority == 3)
            {
                agent.speed = 3.5f;
            }
            else if (interrupt.priority == 4)
            {
                agent.speed = 4.5f;
            }
            else if (interrupt.priority >= 5)
            {
                return Node.Status.Failure;
            }

            goLeaf.executing = true;
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
                {
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

    public Node.Status CanSee()
    {
        if (sight.visibility == 0)
        {
            rig.weight = 0f;
            return Node.Status.Failure;
        }
        rig.weight = 1f;
        enemyManager.UpdatePlayerPosition(player.position);
        //transform.LookAt(new Vector3(player.position.x,transform.position.y,player.position.z));
        return Node.Status.Success;
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

    public Node.Status Reload()
    {
        if (reloading && !animator.GetBool(rlAnim))
        {
            animator.SetBool(rlAnim, true);
            agent.isStopped = true;
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

    public void EndReload()
    {
        weaponController.currentAmmo = weaponController.clipSize;
        animator.SetBool(rlAnim, false);
        reloading = false;
    }

    public Node.Status Chase()
    {
        var s = CanSee();
        if (s == Node.Status.Success)
        {
            agent.speed = 2.5f;
            enemyManager.UpdatePlayerPosition(player.position);
        }
        else
        {
            agent.speed = 3.5f;
        }
        var distance = Vector3.Distance(transform.position, player.position);
        var direction = player.position - transform.position;
        if (distance > 20f)
        {
            agent.speed = 5;
            agent.SetDestination(player.position);
        }
        else if (distance > 5f)
        {
            agent.SetDestination(player.position);
        }
        else if (distance < 5f && Vector3.Angle(transform.forward, direction) > 30f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction.normalized), 5 * Time.deltaTime);
        }

        return s;
    }

    public Node.Status FillSight()
    {
        if (sight.visibility > 0.16f && !enemyManager.ignorePlayer)
        {
            AddInterrupt(5, player.position);
        }
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
        rig.weight = (lookExecuting || (sight.LineOfSight && enemyManager.isAlert)) ? 1 : 0;
        
        sightText.text = ((int)sightTime).ToString();
        visibilityText.text = sight.visibility.ToString("00.00");
        if (stateController.state == EnemyStateController.EnemyState.Normal)
        {
            agent.isStopped = false;
            treeStatus = tree.Process();
        }
        else if (stateController.state == EnemyStateController.EnemyState.Stun)
        {
            sightTime = 0;
            agent.isStopped = true;
        }
        else if (stateController.state == EnemyStateController.EnemyState.Grab)
        {
            transform.position = hostageArea.transform.position;
            transform.rotation = player.rotation;
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
