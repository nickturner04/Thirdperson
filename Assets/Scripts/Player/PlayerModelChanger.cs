using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerModelChanger : MonoBehaviour
{
    [SerializeField] private GameObject playerModel;
    void Start()
    {
        var existingmodel = GameObject.FindGameObjectWithTag("PlayerModel");
        if (existingmodel != null)
        {
            Destroy(existingmodel);
        }
        var model = Instantiate(playerModel,this.transform);
        var controller = GetComponent<PlayerController>();
        var information = model.GetComponent<ModelInformation>();
        var animator = GetComponent<Animator>();
        controller.aimRig = information.rig;
        controller.aimTarget = information.aimTarget;
        animator.avatar = information.avatar;
        GetComponent<WeaponController>().attachPoint = information.attachPoint;
        var camo = GetComponent<FootstepCamoController>();
        camo.bodyParts[0] = information.spine2;
        camo.bodyParts[1] = information.rightLeg2;
        camo.bodyParts[2] = information.leftLeg2;
        camo.bodyParts[3] = information.headMid;
        animator.Rebind();
        animator.Play("Idle", 0);
    }
}
