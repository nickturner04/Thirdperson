using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Baloon : MonoBehaviour
{
    public ShootingRange range;

    public void GetShot()
    {
        range.AddScore(5);
        range.SpawnTarget();
    }
}
