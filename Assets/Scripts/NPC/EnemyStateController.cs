using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStateController : MonoBehaviour
{
    private Animator animator;
    private CapsuleCollider capsuleCollider;
    private Transform player;
    public Transform gAttach;
    [SerializeField] private Rigidbody[] ragdollBoxes;

    public float knockouttimer;
    public float stamina = 100;

    public EnemyState state = EnemyState.Normal;

    public enum EnemyState
    {
        Normal,Stun,Knockout,Grab,Death
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        player = GameObject.FindWithTag("Player").transform;
    }

    private void Update()
    {
        if (knockouttimer > 0)
        {
            if (state != EnemyState.Knockout)
            {
                state = EnemyState.Knockout;
                animator.SetBool("KNOCKOUT", true);
                capsuleCollider.enabled = false;

            }
            
            knockouttimer -= Time.deltaTime;
            
        }
        else if (state == EnemyState.Knockout)
        {
            animator.SetBool("KNOCKOUT", false);
            state = EnemyState.Normal;
            knockouttimer = 0;
            capsuleCollider.enabled = true;
        }
        
    }
    public void SetGrab()
    {
        state = EnemyState.Grab;
        capsuleCollider.isTrigger = true;
        GetComponent<NavMeshAgent>().enabled = false;
        SetRagdoll(false);
    }
    public void EndGrab()
    {
        state = EnemyState.Normal;
        capsuleCollider.isTrigger = false;
        GetComponent<NavMeshAgent>().enabled = true;
        SetRagdoll(true);
        //player.GetComponent<PlayerController>().hostageController = null;
    }

    private void SetRagdoll(bool enabled)
    {
        foreach (var rb in ragdollBoxes)
        {
            rb.GetComponent<Collider>().enabled = enabled;
        }
    }
    public void Stun()
    {
        animator.SetBool("STUNNED",true);

        state = EnemyState.Stun;

        
    }
    public void EndStun()
    {
        GetComponent<EnemyBehaviour>().AddInterrupt(1, player.position);
        animator.SetBool("STUNNED", false);
        state = EnemyState.Normal;
    }
    public void Die()
    {
        state = EnemyState.Death;
        animator.enabled = false;
        capsuleCollider.enabled = false;
        GetComponent<EnemyWeaponController>().EndFire();
        var playercontroller = player.GetComponent<PlayerController>();
        if (playercontroller.hostageController == this)
        {
            playercontroller.RemoveGrabTarget(capsuleCollider);
        }
        foreach (var rb in ragdollBoxes)
        {
            rb.AddForce((rb.position - player.position).normalized * 5000);
        }
    }
    public void TakeStaminaDamage(float damage)
    {
        if (state != EnemyState.Knockout && state != EnemyState.Death)
        {
            stamina -= damage;

            if (stamina <= 0)
            {
                EndStun();
                animator.SetBool("STUNNED", false);
                animator.SetBool("KNOCKOUT", true);
                knockouttimer = 60;
                stamina = 100;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (state != EnemyState.Grab && other.gameObject.layer == 10)
        {
            Hitbox hit = other.gameObject.GetComponent<Hitbox>();
            if (state == EnemyState.Normal)
            {
                Stun();
            }

            TakeStaminaDamage(hit.staminaDamage);
            if (hit.staminaDamage > 50 && TryGetComponent(out Rigidbody rb))
            {
                rb.AddForce((transform.position - other.transform.position).normalized * 1000);
            }
            

        }
    }
}
