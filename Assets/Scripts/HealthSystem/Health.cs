using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public float maxHealth = 100;

    public float health;
    public UnityEvent OnDie;
    public UnityEvent OnTakeDamage;
    public bool godMode;

    public bool isPlayer = false;

    private void OnEnable()
    {
        health = maxHealth;
    }

    public virtual void TakeDamage(float damage)
    {
        if (!godMode)
        {
            if (health <= 0) return;
            health -= damage;
            health = Mathf.Clamp(health, 0, maxHealth);
            OnTakeDamage.Invoke();
            if (health == 0) StartCoroutine(Die());
        }
        
    }

    public void RestoreHealth(float damage)
    {
        health += damage;
        health = Mathf.Clamp(health, 0, maxHealth);
    }

    public IEnumerator Die()
    {
        yield return null;
        //Debug.Log($"{gameObject.name}: Ouch!");
        OnDie.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 10)
        {
            Hitbox hit = other.gameObject.GetComponent<Hitbox>();
            if (isPlayer && !hit.isPlayer)
            {

            }
            else if (!isPlayer && hit.isPlayer)
            {
                TakeDamage(hit.damage);
                if (hit.damage > 50 && TryGetComponent(out Rigidbody rb))
                {
                    rb.AddForce((transform.position - other.transform.position).normalized * 1000);
                }
            }
            
        }
    }
}
