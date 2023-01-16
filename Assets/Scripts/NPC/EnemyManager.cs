using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.AI;

public class EnemyManager : MonoBehaviour
{

    public Vector3 lastPlayerPosition;
    [SerializeField] private float alertTime;
    public float currentAlertTime;
    public bool isAlert;
    public bool ignorePlayer;
    public bool callAlert;
    public bool indefiniteAlert = false;
    private AudioSource source;
    private float timeMultiplier = 1;

    public List<EnemyBehaviour> enemies = new List<EnemyBehaviour>();
    [SerializeField] private GameObject enemy;
    [HideInInspector]public LabelManager labelManager;

    public void Populate(EnemyData[] enemyData)
    {
        var player = GameObject.Find("PlayerV5");
        if (player == null) Debug.Log("NULL");
        for (int i = 0; i < enemyData.Length; i++)
        {
            //Instantiate each enemy in the enemydata array
            EnemyData item = enemyData[i];
            var e = Instantiate(enemy, item.position, Quaternion.Euler(item.rotation)).GetComponent<EnemyBehaviour>();
            e.patrolPoints = item.patrolPoints;
            e.nextPoint = item.nextPoint;
            var s = e.GetComponent<EnemyStateController>();
            s.state = item.state;
            s.stamina = item.stamina;
            s.knockouttimer = item.knockoutTime;
            e.GetComponent<Health>().health = item.health;
            e.enemyManager = this;
            enemies.Add(e);
            if (s.state == EnemyStateController.EnemyState.Grab)
            {//If enemy is grabbed spawn them connected to player and update the player's state
                s.GetComponent<CapsuleCollider>().isTrigger = true;
                s.GetComponent<NavMeshAgent>().enabled = false;
                GameObject.Find("PlayerV5").GetComponent<PlayerController>().hostageController = s;
            }
            
        }
    }

    public void DeleteAllEnemies()
    {
        //Reset Enemy List
        foreach (var item in enemies)
        {
            Destroy(item.gameObject);
        }
        enemies.Clear();
    }

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void CheckAwakeEnemies()
    {//If all enemies are knocked out or dead, make the alert end quicker
        foreach (var item in enemies)
        {
            var state = item.GetComponent<EnemyStateController>().state;
            if (state == EnemyStateController.EnemyState.Death || state == EnemyStateController.EnemyState.Stun)
            {
                timeMultiplier = 5;
                return;
            }
        }
        timeMultiplier = 1;
    }
    //Resets alert time and updates known player position, or calls alert
    public void UpdatePlayerPosition(Vector3 position)
    {
        lastPlayerPosition = position;
        currentAlertTime = alertTime;
        if (!isAlert)
        {
            source.Play();
            CallAlert();
        }
        isAlert = true;
    }

    public void CallAlert()
    {//Change each enemy's behaviour tree to the alert one to change their behaviour
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].GetComponent<EnemyStateController>().state != EnemyStateController.EnemyState.Death)
            {
                EnemyBehaviour item = enemies[i];
                if (i % 2 == 0) enemies[i].surround = true;
                item.tree = item.treeAlert;
                item.agent.SetDestination(item.transform.position);
                item.alertDestination = item.transform.position;
                item.interrupt.priority = 0;
            }
            
        }
        CheckAwakeEnemies();//reset time multiplier
    }

    public void EndAlert()
    {//Change each enemy's behaviour tree back to the normal one
        foreach (EnemyBehaviour item in enemies)
        {
            item.tree = item.treeNormal;
        }    }

    private void Update()
    {
        if (callAlert)
        {
            UpdatePlayerPosition(lastPlayerPosition);
            callAlert = false;
        }
        if (isAlert)
        {
            labelManager.SetAlert(currentAlertTime);
            currentAlertTime -= Time.deltaTime * timeMultiplier * (indefiniteAlert ? 0 : 1);
            if (currentAlertTime <= 0)
            {
                isAlert = false;
                labelManager.HideAlert();
                currentAlertTime = alertTime;
                source.Stop();
                EndAlert();
            }
        }   
    }
}
