using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FootstepCamoController : MonoBehaviour
{
    public Transform[] bodyParts;

    [SerializeField] private GameObject soundMaker;
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
    public bool isInvisible = false; //Debug variable, not used in this script but enemies will ignore player if this is true
    private float noiseLevel;
    private float stepDistance = 25;
    public bool inShadow = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
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

    //This method is called by the player controller whenever the player moves
    public void Move(float speed)
    {
        //Get noise of footstep depending on terrain type and speed
        var noise = speed * layerSoundMultipliers[currentLayer];
        labelManager.SetNoiseLevelPlayer(speed * noise);
        stepDistance -= speed;
        if (stepDistance <= 0 && noise > noiseLevel)
        {
            stepDistance = 100;
            audioSource.PlayOneShot(GetRandomClip());
            var colliders = Physics.OverlapSphere(transform.position,2 * (1 +(noise - noiseLevel)), 1<<8);
            foreach (Collider v in colliders)
            {//If footstep detects enemy in radius make them look that way
                v.GetComponent<EnemyBehaviour>().AddInterrupt(1, transform.position);
            }
        }
    }

    //Return a sound for the appropriate type of ground
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

    //Layer 11 = shadow, when in this layer the player is hidden
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

