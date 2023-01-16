using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{

    private PlayerInput playerInput;
    private InputAction saveAction;
    private InputAction loadAction;
    private InputAction pauseAction;

    [SerializeField] private GameObject playerEssentials;
    [SerializeField] private GameObject[] playerModels;
    [SerializeField] private WeaponData[] weaponMap;
    public GameObject playerEssentialsInstance;

    public SaveData currentSave;
    private EnemyManager enemyManager;
    private Scene currentScene;
    private PlayerController playerController;
    private UIDocument document;
    private bool firstload = true;

    private VisualElement vseGameOver;
    private Button btnContinue;
    private Button btnMainMenu;

    private void OnDisable()
    {
        saveAction.performed -= SaveAction;
        loadAction.performed -= LoadAction;
        pauseAction.performed -= PauseAction;
    }
    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        saveAction = playerInput.actions["Quicksave"];
        loadAction = playerInput.actions["Quickload"];
        pauseAction = playerInput.actions["Pause"];
        saveAction.performed += SaveAction;
        loadAction.performed += LoadAction;
        pauseAction.performed += PauseAction;

        document = GetComponentInChildren<UIDocument>();
        vseGameOver = document.rootVisualElement.Q<VisualElement>("vseGameOver");
        btnContinue = vseGameOver.Q<Button>("btnContinue");
        btnContinue.clicked += Continue;
        btnMainMenu = vseGameOver.Q<Button>("btnMainMenu");
        btnMainMenu.clicked += MainMenu;
        Debug.Log(currentSave.player.ammo.ToString());
        JSave("defaultsave.json", currentSave);
        StartCoroutine(LoadSaveFile("defaultsave.json"));
    }

    private int WeaponDataToMap(WeaponData weapon)
    {

        for (int i = 0; i < weaponMap.Length; i++)
        {
            if (weaponMap[i].displayName == weapon.displayName)
            {
                return i;
            }
        }
        return 0;
    }

    private IEnumerator LoadSaveFile(string path)
    {//Unsubscribe from these so that the player can not call this coroutine multiple times
        saveAction.performed -= SaveAction;
        loadAction.performed -= LoadAction;
        pauseAction.performed -= PauseAction;

        SaveData save = JLoad(path);
        AsyncOperation asyncUnload = new();
        AsyncOperation asyncLoad;
        asyncLoad = SceneManager.LoadSceneAsync(save.room,LoadSceneMode.Additive);
        if (!firstload) 
        {
            //Unload currently loaded scene
            asyncUnload = SceneManager.UnloadSceneAsync(currentScene);
            while (!asyncLoad.isDone && !asyncUnload.isDone)
            {
                //Debug.Log(asyncLoad.progress);
                yield return null;
            }
        }
        else
        {
            while (!asyncLoad.isDone )
            {
                //Debug.Log(asyncLoad.progress);
                yield return null;
            }
            firstload = false;
        }
        //Wait until scene finishes loading before doing this
        asyncLoad.allowSceneActivation = true;
        yield return new WaitForSecondsRealtime(0.1f); //Added delay to prevent errors where the scene takes too long to activate
        currentScene = SceneManager.GetSceneByBuildIndex(save.room);
        SceneManager.SetActiveScene(currentScene);
        InstantiateAll(ref save);

        saveAction.performed += SaveAction;
        loadAction.performed += LoadAction;
        pauseAction.performed += PauseAction;
    }

    private void InstantiateAll(ref SaveData save)
    {//Read the save file and instantiate every object in its stored position
        playerEssentialsInstance = Instantiate(playerEssentials, Vector3.zero, Quaternion.identity);
        try
        {
            enemyManager = GameObject.Find("EnemyManager").GetComponent<EnemyManager>();
            enemyManager.DeleteAllEnemies();
            enemyManager.Populate(save.rooms[save.room].enemies);
        }
        catch (System.Exception)
        {

        }

        playerController = playerEssentialsInstance.transform.Find("PlayerV5").GetComponent<PlayerController>();

        var model = Instantiate(playerModels[GameData.chosencharacter], playerController.transform);
        var information = model.GetComponent<ModelInformation>();
        var animator = model.GetComponent<Animator>();
        playerController.animator = animator;
        playerController.aimRig = information.rig;
        playerController.aimTarget = information.aimTarget;
        playerController.GetComponent<WeaponController>().attachPoint = information.attachPoint;
        playerController.GetComponent<GhostController>().ghostPrefab = information.ghost;
        var camo = playerController.GetComponent<FootstepCamoController>();
        camo.bodyParts[0] = information.spine2;
        camo.bodyParts[1] = information.rightLeg2;
        camo.bodyParts[2] = information.leftLeg2;
        camo.bodyParts[3] = information.headMid;
        animator.Rebind();

        var charcon = playerController.GetComponent<CharacterController>();
        playerController.GetComponent<PlayerHealth>().gameManager = this;
        var inventory = playerController.GetComponent<Inventory>();
        charcon.enabled = false;
        playerController.transform.SetPositionAndRotation(save.player.position, Quaternion.Euler(save.player.rotation));
        charcon.enabled = true;
        if (save.player.isCrouching)
        {
            playerController.StartCrouch();
            animator.Play("CrouchIdle", 0);
        }
        else
        {
            animator.Play("StandIdle", 0);
        }
        playerController.SetMode(save.player.mode);
        playerController.GetComponent<Health>().health = save.player.health;
        for (int i = 0; i < currentSave.player.ammo.Length; i++)
        {
            inventory.ammo[0, i] = currentSave.player.ammoReserve[i];
            inventory.ammo[1, i] = currentSave.player.ammo[i];
        }
        for (int i = 0; i < 3; i++)
        {
            inventory.weapons[i] = weaponMap[save.player.inventory[i]];
        }
        inventory.Equip(save.player.currentWeapon);
        enemyManager.currentAlertTime = 0;
        enemyManager.labelManager = playerEssentialsInstance.transform.Find("UIDocument").GetComponent<LabelManager>();
    }

    public void Transition(int room, Vector3 PlayerPos)
    {//Change Scene
        currentSave.room = room;
        currentSave.player.position = PlayerPos;
        JSave("autosave.json",currentSave);
        StartCoroutine(LoadSaveFile("autosave.json"));
    }

    public void GameOver()
    {
        SceneManager.UnloadSceneAsync(currentScene.buildIndex);
        firstload = true;
        vseGameOver.style.display = DisplayStyle.Flex;
        UnityEngine.Cursor.lockState = CursorLockMode.Confined;
        UnityEngine.Cursor.visible = true;
    }
    private void Continue()
    {
        StartCoroutine(LoadSaveFile("currentsave.json"));
        vseGameOver.style.display = DisplayStyle.None;
        GameObject.FindGameObjectWithTag("SoundManager").GetComponent<AudioListener>().enabled = false;
    }
    private void MainMenu()
    {
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    private void PauseAction(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(0,LoadSceneMode.Single);
    }

    public void LoadAction(InputAction.CallbackContext context)
    {
        StartCoroutine(LoadSaveFile("currentsave.json"));
    }

    public void Save()
    {
        var inventory = playerController.GetComponent<Inventory>();
        var inventoryammo = inventory.ammo;
        var ammo = new int[inventoryammo.GetLength(1)];
        var reserveammo = new int[inventoryammo.GetLength(1)];
        for (int i = 0; i < inventoryammo.GetLength(1); i++)
        {
            reserveammo[i] = inventoryammo[0, i];
            ammo[i] = inventoryammo[1, i];
        }
        var playerdata = new PlayerData(playerController.transform.position, playerController.transform.rotation.eulerAngles, playerController.mode, playerController.isCrouching, (int)playerController.GetComponent<Health>().health, ammo, reserveammo,new int[3], inventory.currentWeapon);
        for (int i = 0; i < inventory.weapons.Length; i++)
        {
            playerdata.inventory[i] = WeaponDataToMap(inventory.weapons[i]);
        }
        var room = SceneManager.GetActiveScene().buildIndex;
        var roomdata = (RoomData[])currentSave.rooms.Clone();
        roomdata[room].enemies = new EnemyData[enemyManager.enemies.Count];
        for (int i = 0; i < enemyManager.enemies.Count; i++)
        {
            var enemy = enemyManager.enemies[i];
            var enemyStateController = enemy.GetComponent<EnemyStateController>();
            if (enemyStateController.state != EnemyStateController.EnemyState.Death)
            {
                roomdata[room].enemies[i] = new EnemyData(enemy.transform.position, enemy.transform.rotation.eulerAngles, enemyStateController.state, enemy.interrupt, (int)enemy.GetComponent<Health>().health, (int)enemyStateController.stamina, enemyStateController.knockouttimer, enemy.nextPoint, enemy.patrolPoints);
            }
            
        }
        currentSave = new SaveData(playerdata, roomdata, room);
        Debug.Log("SAVED");
        JSave("currentsave.json", currentSave);
    }

    private void SaveAction(InputAction.CallbackContext context)
    {
        Save(); 
    }

    public static void JSave(string path,SaveData data)
    {
        File.WriteAllText(path, JsonUtility.ToJson(data,true));
    }

    public static SaveData JLoad(string path)
    {
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
    }
}
