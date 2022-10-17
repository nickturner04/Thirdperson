using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject soundMaker;
    [SerializeField] private float range;
    [SerializeField] private int priority;
    public void Spawn(GameObject prefab)
    {
        Instantiate(prefab, transform.position, Quaternion.identity);
    }
    public void MakeSound()
    {
        var sound = Instantiate(soundMaker,transform.position,Quaternion.identity).GetComponent<SoundMaker>();
        sound.range = range; sound.priority = priority;
    }
}
