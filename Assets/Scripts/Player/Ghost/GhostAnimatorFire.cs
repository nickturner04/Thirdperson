using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GhostAnimatorFire : GhostAnimator
{
    [Header("Character Specific Properties")]
    [SerializeField] private GameObject explosion;
    [SerializeField] private GameObject fireWave;
    [SerializeField] private Transform fireWaveAttach;
    [SerializeField] private AudioClip fireWaveClip;
    private bool charging = false;
    public override void AbilityL1Start(InputAction.CallbackContext context)
    {
        if (!playerController.guard)
        {
            animator.SetBool("Charge", true);
            base.AbilityL1Start(context);
            occupied = true;
            charging = true;
        }
    }

    public override void AbilityL1Stop(InputAction.CallbackContext context)
    {
        if (!playerController.guard && charging)
        {
            base.AbilityL1Stop(context);
            animator.SetBool("Charge", false);
            Instantiate(explosion, playerController.transform.position, Quaternion.identity);
            occupied = false;
            charging = false;
        }
    }
    public override void AbilityL2(InputAction.CallbackContext _)
    {
        if (!playerController.guard && !occupied && playerHealth.energy >= playerHealth.maxEnergy / 2)
        {
            playerHealth.TakeEnergyDamage(playerHealth.maxEnergy / 2);
            animator.Play("Shoot");
            occupied = true;
            ghostActiveTimer = 1f;
        }
    }

    public override void Disappear()
    {
        base.Disappear();
        animator.SetBool("Charge", false);
        if (charging)//if charging cancel charge
        {
            charging = false;
            occupied = false;
        }
    }

    public void StartShoot()
    {
        audioSource.PlayOneShot(fireWaveClip);
        Instantiate(fireWave, fireWaveAttach.position, fireWaveAttach.rotation);
        
    }

    public void EndShoot()
    {
        occupied = false;
    }
}
