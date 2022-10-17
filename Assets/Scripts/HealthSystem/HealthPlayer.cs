using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPlayer : Health
{
    public float resetTime = 20;
    public float maxShield = 250;
    public float shield;
    private float currentResetTime = 0;
    [SerializeField] private float regenSpeed = 1f;

    private void Start()
    {
    }
    public override void TakeDamage(float damage)
    {
        currentResetTime = 0;
        shield -= damage;
        if (shield < 0)
        {
            health += shield;
            shield = 0;
        }
    }

    private void Update()
    {
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
