using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundMaker : MonoBehaviour
{
    public float range;
    public int priority;
    void Start()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, range);
        foreach (var item in colliders)
        {
            if (item.gameObject.TryGetComponent(out EnemyBehaviour enemyBehaviour))
            {
                enemyBehaviour.interrupt = new Interrupt(priority, transform.position);
            }
        }
        Invoke("Cleanup", 3f);
    }

    void Cleanup()
    {
        Destroy(gameObject);
    }
}
