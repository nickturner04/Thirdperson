using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

public class LabelManager : MonoBehaviour
{//This class enables different scripts to update UI elements

    private Label lblAmmo;
    private Label lblCamo;
    private Label lblPhase;
    private Label lblTime;
    private Label lblWeaponName;
    private Label lblNoiseLevel;
    private Label lblPlNoise;
    private VisualElement vseWeaponSprite;
    private VisualElement vseHealth;
    private VisualElement vseShield;
    private VisualElement vseAlert;
    private VisualElement vseButtonPrompts;
    private VisualElement vseKeyBlack;
    private Label lblKey;

    private VisualElement vseAmmo;
    private VisualElement vseWeaponMenu;
    public Button[] vseWeaponButtons = new Button[3];

    private VisualElement vsePickup;
    private Label lblPickup;
    private VisualElement vsePickupSprite;
    string[] labels = new string[] { "Primary", "Secondary", "Sidearm" };

    private bool weaponMenuHidden = true;
    private bool opacityDirection = true;
    [SerializeField] private float mashSpeed = 2f;

    private PlayerController playerController;

    private void Awake()
    {
        playerController = GameObject.Find("PlayerV5").GetComponent<PlayerController>();
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;

        vseButtonPrompts = rootVisualElement.Q<VisualElement>("vseButtonPrompts");
        vseKeyBlack = vseButtonPrompts.Q<VisualElement>("vseKeyBlack");
        lblKey = vseButtonPrompts.Q<Label>("lblKey");
        lblAmmo = rootVisualElement.Q<Label>("lblAmmo");
        lblCamo = rootVisualElement.Q<Label>("lblCamo");
        lblNoiseLevel = rootVisualElement.Q<Label>("lblNoise");
        lblPlNoise = rootVisualElement.Q<Label>("lblPlNoise");
        vseHealth = rootVisualElement.Q<VisualElement>("vseHealthBar");
        vseShield = rootVisualElement.Q<VisualElement>("vseShieldBar");
        vseAlert = rootVisualElement.Q<VisualElement>("vseAlert");
        lblTime = vseAlert.Q<Label>("lblTime");
        vseAmmo = rootVisualElement.Q<VisualElement>("vseAmmo");
        vseWeaponSprite = vseAmmo.Q<VisualElement>("vseWeaponSprite");
        lblWeaponName = vseAmmo.Q<Label>("lblWeaponName");
        vseWeaponMenu = rootVisualElement.Q<VisualElement>("vseWeaponMenu");
        vseWeaponButtons[0] = vseWeaponMenu.Q<Button>("btnPrimary");
        vseWeaponButtons[1] = vseWeaponMenu.Q<Button>("btnSecondary");
        vseWeaponButtons[2] = vseWeaponMenu.Q<Button>("btnSidearm");
        vseAlert.style.display = DisplayStyle.None;
        vsePickup = rootVisualElement.Q<VisualElement>("vsePickup");
        vsePickupSprite = rootVisualElement.Q<VisualElement>("vsePickupSprite");
        lblPickup = rootVisualElement.Q<Label>("lblPickup");

        var btnHeal = vseWeaponMenu.Q<Button>("btnHeal");
        btnHeal.clicked += Heal;

        var btnRespawn = vseWeaponMenu.Q<Button>("btnRespawn");
        btnRespawn.clicked += Respawn;

        var btnGodMode = vseWeaponMenu.Q<Button>("btnGodMode");
        btnGodMode.clicked += ToggleGodMode;
        vseButtonPrompts.style.display = DisplayStyle.None;
    }

    private void Heal()
    {
        var health = GameObject.Find("PlayerV5").GetComponent<Health>();
        health.health = health.maxHealth;
        var inventory = GameObject.Find("PlayerV5").GetComponent<Inventory>();
        inventory.ammo[0, 0] = inventory.maxAmmo[0];
        inventory.ammo[0, 1] = inventory.maxAmmo[1];
        inventory.ammo[0, 2] = inventory.maxAmmo[2];
        UpdateWeaponMenu(inventory.ammo, new Sprite[] { inventory.weapons[0].displaySprite, inventory.weapons[1].displaySprite, inventory.weapons[2].displaySprite });
    }
    
    private void ToggleGodMode()
    {
        var health = playerController.GetComponent<Health>();
        health.godMode = !health.godMode;
        Debug.Log("God Mode: " + health.godMode);
    }

    public void SetPrompt(DisplayStyle style, string key)
    {//Show/Hide Button Prompt
        vseButtonPrompts.style.display = style;
        lblKey.text = key;
    }

    private void Respawn()
    {
    }

    public void SetPickup(DroppedWeapon weapon)
    {
        vsePickup.style.display = DisplayStyle.Flex;
        vsePickupSprite.style.backgroundImage = new StyleBackground(weapon.weaponData.displaySprite);

        lblPickup.text = $"{labels[weapon.weaponData.slot]} - {weapon.ammo}/{weapon.weaponData.clipSize}";
    }

    public void HidePickup()
    {
        vsePickup.style.display = DisplayStyle.None;
    }

    public void SetNoiseLevel(float noise)
    {
        lblNoiseLevel.text = noise.ToString();
    }

    public void SetNoiseLevelPlayer(float noise)
    {
        lblPlNoise.text = noise.ToString();
    }

    public void SetVisibility(bool hidden)
    {
        lblCamo.text = hidden ? "Hidden" : "Visible";
    }

    public void SetAmmo(int ammo, int maxAmmo)
    {
        lblAmmo.text = $"{ammo}/{maxAmmo}";
    }

    public void UpdateAmmo(DisplayStyle style)
    {
        lblAmmo.style.display = style;
    }

    public void SetWeapon(Sprite sprite, string name)
    {
        vseWeaponSprite.style.backgroundImage = new StyleBackground(sprite);
        lblWeaponName.text = name;
    }

    public void SetHealth(float health,float maxhealth)
    {
        vseHealth.style.width = 420 * (health / maxhealth);
        if (vseHealth.style.width == 420)
        {
            vseHealth.style.borderRightWidth = 3;
        }
        else
        {
            vseHealth.style.borderRightWidth = 0;
        }
    }

    public void SetShield(float shield, float maxshield)
    {
        vseShield.style.width = 420 * (shield / maxshield);
    }

    public void SetAlert(float time)
    {
        vseAlert.style.display = DisplayStyle.Flex;
        if (time == 10)
        {
            time = 9.99f;
        }
        lblTime.text = (time * 10).ToString("00.00");
    }

    public void HideAlert()
    {
        vseAlert.style.display = DisplayStyle.None;
    }

    public void Inventory()
    {
        playerController.Inventory(new UnityEngine.InputSystem.InputAction.CallbackContext());

    }

    public void ShowWeaponMenu()
    {
        if (weaponMenuHidden)
        {
            vseWeaponMenu.style.display = DisplayStyle.Flex;
            vseAmmo.style.display = DisplayStyle.None;
        }
        else
        {
            vseWeaponMenu.style.display = DisplayStyle.None;
            vseAmmo.style.display = DisplayStyle.Flex;
        }
        weaponMenuHidden = !weaponMenuHidden;
        
    }
    public void UpdateWeaponMenu(int[,] ammo, Sprite[] sprites)
    {
        for (int i = 0; i < 3; i++)
        {
            vseWeaponButtons[i].text = $"{ammo[1, i]}/{ammo[0,i]}";
            vseWeaponButtons[i].style.backgroundImage = new StyleBackground(sprites[i]);
        }
    }
    //private void Update()
    //{
    //    //lblKey.style.color = Mathf.Lerp(vseKeyBlack.style.opacity.value, opacityDirection == true ? 1.50f : 0f, Time.deltaTime * mashSpeed);
    //    //if (vseKeyBlack.style.opacity.value <= 0.52f || vseKeyBlack.style.opacity.value >= 0.98f)
    //    //{
    //    //    opacityDirection = !opacityDirection;
    //    //}
    //}
}
