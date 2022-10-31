using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbiencePlayer : MonoBehaviour
{
    [SerializeField] private GameObject soundMaker;
    AudioSource source;
    bool playing = false;
    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    public void Toggle()
    {
        if (playing)
        {
            source.Stop();
            soundMaker.SetActive(false);
        }
        else
        {
            soundMaker.SetActive(true);
            source.Play();
        }
    }
}
