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
    public GameManager.EnemyData[] enemyData;
    [SerializeField] private GameObject enemy;
    public LabelManager labelManager;

    private void Start()
    {
        Populate(enemyData);
    }

    public void Populate(GameManager.EnemyData[] enemyData)
    {
        var player = GameObject.Find("PlayerV5");
        if (player == null) Debug.Log("NULL");
        for (int i = 0; i < enemyData.Length; i++)
        {
            
            GameManager.EnemyData item = enemyData[i];
            var e = Instantiate(enemy, GameManager.SurrogateToVector(item.position), GameManager.SurrogateToQuaternion(item.rotation)).GetComponent<EnemyBehaviour>();
            e.patrolPoints = GameManager.SurrogateToVectorArray(item.patrolPoints);
            e.nextPoint = item.nextPoint;
            var s = e.GetComponent<EnemyStateController>();
            s.state = item.state;
            s.stamina = item.stamina;
            s.knockouttimer = item.knockoutTime;
            e.GetComponent<Health>().health = item.health;
            e.enemyManager = this;
            enemies.Add(e);
            if (s.state == EnemyStateController.EnemyState.Grab)
            {
                s.GetComponent<CapsuleCollider>().isTrigger = true;
                s.GetComponent<NavMeshAgent>().enabled = false;
                GameObject.Find("PlayerV5").GetComponent<PlayerController>().hostageController = s;
            }
            
        }
    }

    public void DeleteAllEnemies()
    {
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
    {
        foreach (var item in enemies)
        {
            var state = item.GetComponent<EnemyStateController>().state;
            if (state == EnemyStateController.EnemyState.Normal || state == EnemyStateController.EnemyState.Stun)
            {
                timeMultiplier = 1;
                return;
            }
        }
        timeMultiplier = 5;
    }
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
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].GetComponent<EnemyStateController>().state != EnemyStateController.EnemyState.Death)
            {
                EnemyBehaviour item = enemies[i];
                if (i % 2 == 0) enemies[i].surround = true;
                item.tree = item.treeAlert;
                item.agent.SetDestination(item.transform.position);
                item.interrupt.priority = 0;
            }
            
        }
    }

    public void EndAlert()
    {
        foreach (EnemyBehaviour item in enemies)
        {
            item.tree = item.treeNormal;
        }
    }

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

public enum phase
{
    normal,search,alert
}
