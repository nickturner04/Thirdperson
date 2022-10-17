using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private int room;
    [SerializeField] private Vector3 position;
    public void Transition()
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().Transition(room, position);
    }
}
