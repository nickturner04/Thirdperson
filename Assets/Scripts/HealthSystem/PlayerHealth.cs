using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerHealth : MonoBehaviour
{
    private Health health;
    public GameManager gameManager;
    private GhostController ghostController;
    [SerializeField] private LabelManager labelManager;

    public float resetTime = 20;
    public float maxShield = 250;
    public float shield;
    private float currentResetTime = 0;
    [SerializeField] private float regenSpeed = 1f;


    private void Awake()
    {
        health = GetComponent<Health>();
        ghostController = GetComponent<GhostController>();
    }

    public void TakeDamage(float damage, Vector3 position)
    {
        if ((shield > 0 && !ghostController.occupied) || health.godMode)
        {//If there is shield, take away damage from the shield and move the ghost to where the bullet hit.
            ghostController.Guard();
            ghostController.ghostActiveTimer = 0.1f;
            ghostController.gTransform.SetPositionAndRotation(transform.position + (position - transform.position).normalized, Quaternion.LookRotation(position - transform.position));
        }
        if (health.godMode) return;
        currentResetTime = 0;
        shield -= damage;
        if (shield < 0)
        {
            health.TakeDamage(damage);
            shield = 0;
        }
        if (health.health <= 0)
        {//Trigger Game Over
            Die ();
        }
    }

    public void Die()
    {
        gameManager.GameOver();
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
