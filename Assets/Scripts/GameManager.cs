using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.UIElements;

[System.Serializable]
public struct V3Surrogate
{//Vector3 with unnecesary data stripped out
    public V3Surrogate(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public struct InterruptSurrogate
{
    // Replace interrupt with version that does not use Vector3
    public InterruptSurrogate(int priority, V3Surrogate position)
    {
        this.priority = priority;
        this.position = position;
    }
    public int priority;
    public V3Surrogate position;

    public static InterruptSurrogate FromInterrupt(Interrupt interrupt)
    {
        return new InterruptSurrogate(interrupt.priority, GameManager.VectorToSurrogate(interrupt.position));
    }
    public Interrupt ToInterrupt()
    {
        return new Interrupt(priority,GameManager.SurrogateToVector(position));
    }
}

public class GameManager : MonoBehaviour
{
    public static Vector3 SurrogateToVector(V3Surrogate input)
    {
        return new Vector3(input.x, input.y, input.z);
    }

    public static V3Surrogate VectorToSurrogate(Vector3 input)
    {
        return new V3Surrogate(input.x, input.y, input.z);
    }

    public static Vector3[] SurrogateToVectorArray(V3Surrogate[] input)
    {
        var output = new Vector3[input.Length];
        for (int i = 0; i < input.Length; i++)
        {
            output[i] = SurrogateToVector(input[i]);
        }
        return output;
    }

    public static V3Surrogate[] VectorToSurrogateArray(Vector3[] input)
    {
        var output = new V3Surrogate[input.Length];
        for (int i = 0; i < input.Length; i++)
        {
            output[i] = VectorToSurrogate(input[i]);
        }
        return output;
    }

    public static Quaternion SurrogateToQuaternion(V3Surrogate input)
    {
        return Quaternion.Euler(input.x, input.y, input.z);
    }

    public static V3Surrogate QuaternionToSurrogate(Quaternion input)
    {
        return new V3Surrogate(input.eulerAngles.x, input.eulerAngles.y, input.eulerAngles.z);
    }

    private PlayerInput playerInput;
    private InputAction saveAction;
    private InputAction loadAction;
    private InputAction pauseAction;

    [SerializeField] private GameObject playerEssentials;
    [SerializeField] private GameObject[] playerModels;
    [SerializeField] private Scene mainMenu;
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

    private IEnumerator LoadSaveFile(string path)
    {//Unsubscribe from these so that the player can not call this coroutine multiple times
        saveAction.performed -= SaveAction;
        loadAction.performed -= LoadAction;
        pauseAction.performed -= PauseAction;

        SaveData save = JLoad(path);
        AsyncOperation asyncUnload = new AsyncOperation();
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
        animator.Play("Idle", 0);

        var charcon = playerController.GetComponent<CharacterController>();
        playerController.GetComponent<PlayerHealth>().gameManager = this;
        var inventory = playerController.GetComponent<Inventory>();
        charcon.enabled = false;
        playerController.transform.SetPositionAndRotation(SurrogateToVector(save.player.position), SurrogateToQuaternion(save.player.rotation));
        charcon.enabled = true;
        if (save.player.isCrouching)
        {
            playerController.StartCrouch();
            animator.Play("Idle", 1);
        }
        playerController.SetMode(save.player.mode);
        playerController.GetComponent<Health>().health = save.player.health;
        for (int i = 0; i < currentSave.player.ammo.Length; i++)
        {
            inventory.ammo[0, i] = currentSave.player.ammoReserve[i];
            inventory.ammo[1, i] = currentSave.player.ammo[i];
        }
        inventory.Equip(save.player.currentWeapon);
        enemyManager.currentAlertTime = 0;
        enemyManager.labelManager = playerEssentialsInstance.transform.Find("UIDocument").GetComponent<LabelManager>();
    }

    public void Transition(int room, Vector3 PlayerPos)
    {//Change Scene
        currentSave.room = room;
        currentSave.player.position = VectorToSurrogate(PlayerPos);
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
        var playerdata = new PlayerData(VectorToSurrogate(playerController.transform.position), QuaternionToSurrogate(playerController.transform.rotation), playerController.mode, playerController.isCrouching, (int)playerController.GetComponent<Health>().health, ammo, reserveammo, inventory.currentWeapon);
        var room = SceneManager.GetActiveScene().buildIndex;
        var roomdata = (RoomData[])currentSave.rooms.Clone();
        roomdata[room].enemies = new EnemyData[enemyManager.enemies.Count];
        for (int i = 0; i < enemyManager.enemies.Count; i++)
        {
            var enemy = enemyManager.enemies[i];
            var enemyStateController = enemy.GetComponent<EnemyStateController>();
            if (enemyStateController.state != EnemyStateController.EnemyState.Death)
            {
                roomdata[room].enemies[i] = new EnemyData(VectorToSurrogate(enemy.transform.position), QuaternionToSurrogate(enemy.transform.rotation), enemyStateController.state, InterruptSurrogate.FromInterrupt(enemy.interrupt), (int)enemy.GetComponent<Health>().health, (int)enemyStateController.stamina, enemyStateController.knockouttimer, enemy.nextPoint, VectorToSurrogateArray(enemy.patrolPoints));
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
        File.WriteAllText(path, JsonUtility.ToJson(data));
    }

    public static SaveData JLoad(string path)
    {
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
    }
}
