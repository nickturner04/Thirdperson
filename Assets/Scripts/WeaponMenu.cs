using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class WeaponMenu : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private GameObject[] weaponButtons;

    [SerializeField] private PlayerInput playerInput;

    private void OnEnable()
    {
        DisplayWeaponMenu();
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void OnDisable()
    {
        //HideWeaponMenu();
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    private void DisplayWeaponMenu()
    {
        Vector3 position = new Vector3(482,-232);
        for (int i = 0; i < inventory.weapons.Length; i++)
        {
            
            GameObject newWeaponButton = weaponButtons[i];
            TMP_Text newWeaponText = newWeaponButton.GetComponentInChildren<TMP_Text>();
            WeaponButton newWeaponData = newWeaponButton.GetComponent<WeaponButton>();
            newWeaponData.inventory = inventory;
            newWeaponData.weaponIndex = i;
            newWeaponText.text = inventory.weapons[i].name;

            position.x += 50;
            
        }
    }

    private void HideWeaponMenu()
    {
        foreach (Transform item in gameObject.transform)
        {
            Destroy(item.gameObject);
        }
    }
}
