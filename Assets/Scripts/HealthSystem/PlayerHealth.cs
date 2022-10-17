using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerHealth : MonoBehaviour
{
    private HealthPlayer health;
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
        {
            ghostController.Guard();
            ghostController.ghostActiveTimer = 0.1f;
            ghostController.gTransform.position = transform.position + (position - transform.position).normalized;
            ghostController.gTransform.rotation = Quaternion.LookRotation(position - transform.position);
        }
        if (health.godMode) return;
        health.TakeDamage(damage);
    }

    private void Update()
    {
        labelManager.SetHealth(health.health, health.maxHealth);
        labelManager.SetShield(health.shield, health.maxShield);
    }

}
