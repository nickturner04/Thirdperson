using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChangeCamo : MonoBehaviour
{
    private FootstepCamoController camoController;
    [SerializeField] private TMP_InputField l, a, b;

    private void Awake()
    {
        camoController = GameObject.Find("PlayerV5").GetComponent<FootstepCamoController>();
    }

    public void SetCamo()
    {
        camoController.ChangeUniform(float.Parse(l.text), float.Parse(a.text), float.Parse(b.text));
    }
}
