using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAnim : MonoBehaviour
{
    public ParticleSystem muzzleFlash;

    public void Play()
    {
        muzzleFlash.Play();
    }
}
