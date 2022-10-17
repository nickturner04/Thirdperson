using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShootingRange : MonoBehaviour
{
    [SerializeField]private float time = 30;
    [SerializeField] private GameObject target;
    [SerializeField] private GameObject button;
    [SerializeField] private Transform boundary;
    private AudioSource source;
    [SerializeField] private AudioClip clipHit;
    [SerializeField] private AudioClip clipMiss;
    [SerializeField] private TMP_Text txtScore;
    [SerializeField] private TMP_Text txtTime;
    private int score = 0;
    WaitForSeconds oneSecond = new WaitForSeconds(1);
    private GameObject balloon;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void StartGame()
    {
        SpawnTarget();
        txtScore.enabled = true;
        txtTime.enabled = true;
        txtScore.text = "Score: 0";
        txtTime.text = "Time: 30";
        StartCoroutine(TimerTick());
        time = 30;
        score = 0;
        button.SetActive(false);
    }

    public void ResetScore()
    {
        time = 30;
        score = 0;
    }

    public void AddScore(int score)
    {
        this.score += score;
        if (score > 0)
        {
            source.PlayOneShot(clipHit);
        }
        else
        {
            source.PlayOneShot(clipMiss);
        }
        txtScore.text = "Score: " + this.score;
        if (this.score < 0)
            this.score = 0;
        
    }

    public void SpawnTarget()
    {
        var newPosition = new Vector3(boundary.position.x + Random.Range(0, 9f), 2.25f + Random.Range(-1f,1f), boundary.position.z + Random.Range(0, 6f));
        GameObject balloon = Instantiate(target, newPosition, Quaternion.identity);
        var baloonData = balloon.GetComponent<Baloon>();
        Destroy(this.balloon);
        this.balloon = balloon;
        baloonData.range = this;
    }

    private IEnumerator TimerTick()
    {
        while (true)
        {
            
            txtTime.text = "Time: " + time;
            time--;
            yield return oneSecond;
        }
        
    }

    private void Update()
    {
        if (time == 0)
        {
            Destroy(balloon);
            StopAllCoroutines();
            button.SetActive(true);
            txtTime.text = "Training Over, Press the Button Again To Start";
        }
    }
}
