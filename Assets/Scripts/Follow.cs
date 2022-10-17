using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    private Transform trfCamera;
    private Transform trfPlayer;
    void Start()
    {
        trfCamera = Camera.main.transform;
        trfPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(-trfCamera.position.x,trfPlayer.position.y,-trfCamera.position.z);
        transform.rotation = Quaternion.Euler(0, trfCamera.rotation.y, 0);
    }
}
