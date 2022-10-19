using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sight : MonoBehaviour
{
    [Header("Sight Values")]
    [SerializeField] private float maxSightDistance = 50;
    [SerializeField] private float minSightDistance = 2;
    [SerializeField] private float sightAngle = 45;

    [Header("Objects")]
    public Transform trfEye;
    [SerializeField] private LayerMask lmaTerrain;

    public Transform trfPlayer;
    private FootstepCamoController camo;

    public bool LineOfSight;
    public float visibility;
    public float angle;

    private float GetVisibility()
    {
        int max = camo.bodyParts.Length;
        int count = 0;
        Vector3 direction;
        Vector3 part;
        float distance;
        var sightdistance = camo.inShadow ? minSightDistance : maxSightDistance;
        for (int i = 0; i < max; i++)
        {
            part = camo.bodyParts[i].position;
            direction = part - trfEye.position;
            distance = Vector3.Distance(part, trfEye.position);
            if (distance <= sightdistance 
                && Vector3.Angle(trfEye.forward,direction) <= sightAngle 
                && !Physics.Raycast(trfEye.position,direction,distance,lmaTerrain))
            {
                
                count++;
            }
        }
        
        return count / (float)max;
    }

    private void Awake()
    {
        trfPlayer = GameObject.Find("PlayerV5").GetComponent<Transform>();
        camo = GameObject.Find("PlayerV5").GetComponent<FootstepCamoController>();
    }

    private void Update()
    {
        visibility = camo.isInvisible ? 0 : GetVisibility();
        if (visibility != 0) LineOfSight = true;
        else LineOfSight = false;
    }
        
}
