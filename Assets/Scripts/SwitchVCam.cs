using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class SwitchVCam : MonoBehaviour
{
    [SerializeField]
    private GameObject reticle;
    [SerializeField]
    private WeaponController weaponController;
    private CinemachineVirtualCamera virtualCamera;

    [SerializeField] private CinemachineInputProvider[] cinemachineInputProviders;
    [SerializeField] private InputActionReference xy;

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        
    }

    public void StartAim()
    {
        virtualCamera.Priority = 10;
        StartCoroutine(aimTimer());
    }

    public void StopAim()
    {
        virtualCamera.Priority = 8;
        //reticle.SetActive(false);
        StopAllCoroutines();
        weaponController.aiming = false;
    }

    private IEnumerator aimTimer()
    {
        yield return new WaitForSeconds(0.3f);
        //reticle.SetActive(true);
        weaponController.aiming = true;
    }

    public void DisableInput()
    {
        foreach (var item in cinemachineInputProviders)
        {
            item.XYAxis = null;
        }
    }
    public void EnableInput()
    {
        foreach (var item in cinemachineInputProviders)
        {
            item.XYAxis = xy;
        }
    }
}
