using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class SwitchVCam : MonoBehaviour
{
    //This script is attached to the camera object.
    [SerializeField]
    private GameObject reticle;
    [SerializeField]
    private WeaponController weaponController;
    private CinemachineVirtualCamera virtualCamera;

    [SerializeField] private CinemachineInputProvider[] cinemachineInputProviders;
    [SerializeField] private InputActionReference xy;//Reference to the player's input system

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        
    }

    public void StartAim()
    {
        virtualCamera.Priority = 10;
        StartCoroutine(AimTimer());
    }

    public void StopAim()
    {
        virtualCamera.Priority = 8;
        //reticle.SetActive(false);
        StopAllCoroutines();
        weaponController.aiming = false;
    }

    private IEnumerator AimTimer()
    {
        yield return new WaitForSeconds(0.3f);
        //reticle.SetActive(true);
        weaponController.aiming = true;
    }

    //Remove Mouse Input From Camera
    public void DisableInput()
    {
        foreach (var item in cinemachineInputProviders)
        {
            item.XYAxis = null; //Set input system to null
        }
    }

    //Add Mouse input to camera
    public void EnableInput()
    {
        foreach (var item in cinemachineInputProviders)
        {
            item.XYAxis = xy; //Return input system to normal
        }
    }
}
