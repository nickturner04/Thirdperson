using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPosition : MonoBehaviour
{
    private Transform follow;
    void Start()
    {
        follow = transform.parent;
        transform.parent = null;
        transform.rotation = Quaternion.identity;
    }

    private void LateUpdate()
    {
        transform.position = follow.position;
    }


}
