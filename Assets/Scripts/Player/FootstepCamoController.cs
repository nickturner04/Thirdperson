using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FootstepCamoController : MonoBehaviour
{
    public Transform[] bodyParts;

    [SerializeField] private GameObject soundMaker;
    private PlayerController playerController;
    [SerializeField] private LabelManager labelManager;

    [SerializeField]
    private AudioClip[] clipsDirt;
    [SerializeField]
    private AudioClip[] clipsStone;
    [SerializeField]
    private float[] layerSoundMultipliers;

    private AudioSource audioSource;
    private TerrainDetector terrainDetector;
    public List<SoundEmitter> SoundEmitters;

    public int currentLayer;
    public bool isInvisible = false;
    private float noiseLevel;
    private float stepDistance = 25;
    public bool inShadow = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        playerController = GetComponent<PlayerController>();
        terrainDetector = new TerrainDetector();
    }

    private float CalculateAmbientNoiseLevel()
    {
        var noise = 1f;
        foreach (var item in SoundEmitters)
        {
            noise += item.noiseLevel;
        }
        return noise;
    }

    public void Step(float speed)
    {
        
        if (speed * layerSoundMultipliers[currentLayer] >= noiseLevel)
        {
            Debug.Log($"{speed * layerSoundMultipliers[currentLayer]} > {noiseLevel}");
        }
    }

    public void Move(float speed)
    {
        var noise = speed * layerSoundMultipliers[currentLayer];
        labelManager.SetNoiseLevelPlayer(speed * noise);
        stepDistance -= speed;
        if (stepDistance <= 0 && noise > noiseLevel)
        {
            stepDistance = 100;
            audioSource.PlayOneShot(GetRandomClip());
            var colliders = Physics.OverlapSphere(transform.position,2 * (1 +(noise - noiseLevel)), 1<<8);
            foreach (Collider v in colliders)
            {
                v.GetComponent<EnemyBehaviour>().AddInterrupt(1, transform.position);
            }
        }
    }

    private AudioClip GetRandomClip()
    {
        switch (currentLayer)
        {
            case 0: return clipsDirt[Random.Range(0, clipsDirt.Length)];
            case 1: return clipsDirt[Random.Range(0, clipsDirt.Length)];
            case 2: return clipsStone[Random.Range(0, clipsStone.Length)];
            default: return null;
        }
    }

    private void Update()
    {
        try
        {
            var nextLayer = terrainDetector.GetActiveTerrainTextureIdx(transform.position);    
        }
        catch (System.Exception){}
        noiseLevel = CalculateAmbientNoiseLevel();
        labelManager.SetNoiseLevel(noiseLevel);

        //imgCamo.color = new Color(imgCamo.color.r, imgCamo.color.g, imgCamo.color.b, Mathf.Lerp(imgCamo.color.a, camoAlpha, Time.deltaTime * 2.5f));
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 11)
        {
             inShadow = true;
            labelManager.SetVisibility(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 11)
        {
            inShadow = false;
            labelManager.SetVisibility(false);
        }
    }

}

