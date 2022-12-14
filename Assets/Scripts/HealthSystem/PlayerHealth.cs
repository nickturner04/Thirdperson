using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.DualShock;

[RequireComponent(typeof(Health))]
public class PlayerHealth : MonoBehaviour
{
    private Health health;
    public GameManager gameManager;
    private GhostController ghostController;
    private PlayerController playerController;
    [SerializeField] private LabelManager labelManager;

    public float resetTime = 20;
    public float maxShield = 250;
    public float shield;
    private float currentResetTime = 0;
    [SerializeField] private float regenSpeed = 1f;

    bool dead = false;

    public Color currentColor;
    private void Awake()
    {
        ghostController = GetComponent<GhostController>();
        playerController = GetComponent<PlayerController>();
        health = GetComponent<Health>();
    }

    public void TakeDamage(float damage, Vector3 position)
    {
        if (health.godMode) return;
        if ((shield > 0 && ghostController.guard))
        {//If there is shield, take away damage from the shield and move the ghost to where the bullet hit.
            ghostController.Guard();
            ghostController.ghostActiveTimer = 0.1f;
            ghostController.gTransform.SetPositionAndRotation(transform.position + (position - transform.position).normalized, Quaternion.LookRotation(position - transform.position));
            shield -= damage;
        }
        else
        {
            health.TakeDamage(damage);
        }
        if (shield < 0)
        {
            ghostController.guard = false;
            shield = 0;
        }
        
        currentResetTime = 0;
        
        //if (health.health <= 0 && !dead)
        //{//Trigger Game Over
        //    Die ();
        //    dead = true;
        //}
    }

    public void Die()
    {
        playerController.Die();
    }

    private void Update()
    {

        labelManager.SetHealth(health.health, health.maxHealth);
        labelManager.SetShield(shield, maxShield);

        currentResetTime += Time.deltaTime;
        if (currentResetTime >= resetTime)
        {
            //shield = Mathf.Lerp(shield, maxShield * 2, Time.deltaTime * regenSpeed);
            shield += Time.deltaTime * regenSpeed;
        }
        if (shield > maxShield) shield = maxShield;
        if (currentResetTime > resetTime) currentResetTime = resetTime;
    }
}