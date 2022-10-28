using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerHealth : MonoBehaviour
{
    private HealthPlayer health;
    public GameManager gameManager;
    private GhostController ghostController;
    [SerializeField] private LabelManager labelManager;

    private void Awake()
    {
        health = GetComponent<HealthPlayer>();
        ghostController = GetComponent<GhostController>();
    }

    public void TakeDamage(float damage, Vector3 position)
    {
        if ((health.shield > 0 && !ghostController.occupied) || health.godMode)
        {//If there is shield, take away damage from the shield and move the ghost to where the bullet hit.
            ghostController.Guard();
            ghostController.ghostActiveTimer = 0.1f;
            ghostController.gTransform.position = transform.position + (position - transform.position).normalized;
            ghostController.gTransform.rotation = Quaternion.LookRotation(position - transform.position);
        }
        if (health.godMode) return;
        health.TakeDamage(damage);
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
        labelManager.SetShield(health.shield, health.maxShield);
    }

}
