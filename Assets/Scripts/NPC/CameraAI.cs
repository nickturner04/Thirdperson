using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAI : MonoBehaviour
{
    private Sight sight;
    private EnemyManager enemyManager;

    private void Awake()
    {
        sight = GetComponent<Sight>();
        enemyManager = GameObject.Find("EnemyManager").GetComponent<EnemyManager>();
    }

    private void Update()
    {
        if (sight.visibility > 0)
        {
            enemyManager.UpdatePlayerPosition(sight.trfPlayer.position);
        }
    }
}
