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
    private LabColor[] layerColors;
    [SerializeField]
    private float[] layerSoundMultipliers;

    private LabColor currentUniform = new(70f,-58f,55f);

    private AudioSource audioSource;
    private TerrainDetector terrainDetector;
    public List<SoundEmitter> SoundEmitters;

    public int currentLayer;
    public bool isInvisible = false;
    public float camoIndex = 0;
    public float trueCamoIndex = 0;
    private float displayCamoIndex = 0;
    private float noiseLevel;
    private float noisePlayer;
    private float stepDistance = 25;

    private bool inPlants = false;

    Color camoColor;
    float camoAlpha = 1;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        playerController = GetComponent<PlayerController>();
        terrainDetector = new TerrainDetector();
        //imgCamo.color = currentUniform.ToColor();
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

    public void ChangeUniform(float l, float a, float b)
    {
        currentUniform = new LabColor(l, a, b);
        CalculateCamo();
    }

    private void CalculateCamo()
    {
        
            camoIndex = Mathf.Clamp(LabColor.DeltaE(currentUniform, layerColors[currentLayer]) * 1.2f, 0f, 100f);

            camoAlpha = Mathf.Clamp(camoIndex * 0.01f, 0.25f, 1f);
            //Debug.Log(camoIndex);
        
    }

    private void Update()
    {
        try
        {
            var nextLayer = terrainDetector.GetActiveTerrainTextureIdx(transform.position);
            if (nextLayer != currentLayer)
            {
                currentLayer = nextLayer;
                CalculateCamo();
            }
                
        }
        catch (System.Exception)
        {
            camoIndex = 0;
        }
        trueCamoIndex = Mathf.Clamp(100 - camoIndex, 0, playerController.isCrouching ? (inPlants ? 100 : 85) : 55);
        displayCamoIndex = Mathf.Lerp(displayCamoIndex, trueCamoIndex, Time.deltaTime * 5f);
        labelManager.SetCamo((int)displayCamoIndex);
        noiseLevel = CalculateAmbientNoiseLevel();
        labelManager.SetNoiseLevel(noiseLevel);

        //imgCamo.color = new Color(imgCamo.color.r, imgCamo.color.g, imgCamo.color.b, Mathf.Lerp(imgCamo.color.a, camoAlpha, Time.deltaTime * 2.5f));
    }

}

[System.Serializable]
public struct LabColor
{
    public LabColor(float l,float a,float b)
    {
        this.l = l;
        this.a = a;
        this.b = b;
    }

    public float l;
    public float a;
    public float b;

    public static float DeltaE(LabColor p, LabColor q)
    {
        float l = (p.l - q.l) * (p.l - q.l);
        float a = (p.a - q.a) * (p.a - q.a);
        float b = (p.b - q.b) * (p.b - q.b);

        return Mathf.Sqrt(l + a + b) ;
    }

    public Color ToColor()
    {
        System.Numerics.Vector4 color = LabToRGB(new System.Numerics.Vector4(l,a,b,255));
        return new Color(color.X,color.Y,color.Z);
    }

    public static System.Numerics.Vector4 LabToXYZ(System.Numerics.Vector4 color)
    {
        float[] xyz = new float[3];
        float[] col = new float[] { color.X, color.Y, color.Z };
        xyz[1] = (col[0] + 16.0f) / 116.0f;
        xyz[0] = (col[1] / 500.0f) + xyz[0];
        xyz[2] = xyz[0] - (col[2] / 200.0f);

        for (int i = 0; i < 3; i++)
        {
            float pow = xyz[i] * xyz[i] * xyz[i];
            if (pow > .008856f)
            {
                xyz[i] = pow;
            }
            else
            {
                xyz[i] = (16.0f / 116.0f) / 7.787f;
            }
        }

        xyz[0] = xyz[0] * 95.047f;
        xyz[1] = xyz[1] * 100.0f;
        xyz[2] = xyz[2] * 108.883f;

        return new System.Numerics.Vector4(xyz[0], xyz[1], xyz[2], color.W);
    }

    public static System.Numerics.Vector4 XYZToRGB(System.Numerics.Vector4 color)
    {
        float[] rgb = new float[3];
        float[] xyz = new float[3];
        float[] col = new float[] { color.X, color.Y, color.Z };

        for (int i = 0; i < 3; i++)
        {
            xyz[i] = col[i] / 100.0f;
        }

        rgb[0] = (xyz[0] * 3.240479f) + (xyz[1] * -1.537150f) + (xyz[2] * -.498535f);
        rgb[1] = (xyz[0] * -.969256f) + (xyz[1] * 1.875992f) + (xyz[2] * .041556f);
        rgb[2] = (xyz[0] * .055648f) + (xyz[1] * -.204043f) + (xyz[2] * 1.057311f);

        for (int i = 0; i < 3; i++)
        {
            if (rgb[i] > .0031308f)
            {
                rgb[i] = (1.055f * (float)Mathf.Pow(rgb[i], 1.0f / 2.4f)) - .055f;
            }
            else
            {
                rgb[i] = rgb[i] * 12.92f;
            }
        }

        return new System.Numerics.Vector4(rgb[0], rgb[1], rgb[2], color.W);
    }


    public static System.Numerics.Vector4 LabToRGB(System.Numerics.Vector4 color)
    {
        System.Numerics.Vector4 xyz = LabToXYZ(color);
        return XYZToRGB(xyz);
    }
}
