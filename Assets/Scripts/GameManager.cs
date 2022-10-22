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
{
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
public struct QuatSurrogate
{
    public QuatSurrogate(float x, float y, float z)
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

    public static Quaternion SurrogateToQuaternion(QuatSurrogate input)
    {
        return Quaternion.Euler(input.x, input.y, input.z);
    }

    public static QuatSurrogate QuaternionToSurrogate(Quaternion input)
    {
        return new QuatSurrogate(input.eulerAngles.x, input.eulerAngles.y, input.eulerAngles.z);
    }

    [System.Serializable]
    public struct EnemyData
    {
        public EnemyData(V3Surrogate position, QuatSurrogate rotation,EnemyStateController.EnemyState state,InterruptSurrogate interrupt, int health, int stamina, float knockoutTime, int nextPoint, V3Surrogate[] patrolPoints)
        {
            this.position = position;
            this.rotation = rotation;
            this.state = state;
            this.interrupt = interrupt;
            this.health = health;
            this.stamina = stamina;
            this.knockoutTime = knockoutTime;
            this.nextPoint = nextPoint;
            this.patrolPoints = patrolPoints;
        }

        public V3Surrogate position;
        public QuatSurrogate rotation;
        public EnemyStateController.EnemyState state;
        public InterruptSurrogate interrupt;
        public int health;
        public int stamina;
        public float knockoutTime;
        public int nextPoint;
        public V3Surrogate[] patrolPoints;
    }
    [System.Serializable]
    public struct RoomData
    {
        public RoomData(EnemyData[] enemies, bool entered)
        {
            this.entered = entered;
            this.enemies = enemies;
        }
        public EnemyData[] enemies;
        public bool entered;
    }
    [System.Serializable]
    public struct PlayerData
    {
        public PlayerData(V3Surrogate position, QuatSurrogate rotation, PlayerController.Mode mode, bool isCrouching, int health, int[,] ammo, int currentWeapon)
        {
            this.position = position;
            this.rotation = rotation;
            this.mode = mode;
            this.isCrouching = isCrouching;
            this.health = health;
            this.ammo = ammo;
            this.currentWeapon = currentWeapon;
        }

        public V3Surrogate position;
        public QuatSurrogate rotation;
        public PlayerController.Mode mode;
        public bool isCrouching;

        public int health;
        public int[,] ammo;
        public int currentWeapon;
    }
    [System.Serializable]
    public struct SaveData
    {
        public SaveData(PlayerData player, RoomData[] rooms,int room)
        {
            this.player = player;
            this.rooms = rooms;
            this.room = room;
        }
        public PlayerData player;
        public RoomData[] rooms;
        public int room;
    }
    private PlayerInput playerInput;
    private InputAction saveAction;
    private InputAction loadAction;
    private InputAction pauseAction;

    [SerializeField] private GameObject playerEssentials;
    [SerializeField] private Scene mainMenu;
    public GameObject playerEssentialsInstance;

    public SaveData currentSave;
    private EnemyManager enemyManager;
    private Scene currentScene;
    private PlayerController playerController;
    private UIDocument document;
    private bool quit = false;
    private bool firstload = true;
    private float playTime = 0;

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

        JSave("defaultsave.json", currentSave);
        StartCoroutine(LoadSaveFile("defaultsave.json"));
    }

    private void Update()
    {
        playTime += Time.deltaTime;
    }

    private IEnumerator LoadSaveFile(string path)
    {
        saveAction.performed -= SaveAction;
        loadAction.performed -= LoadAction;
        pauseAction.performed -= PauseAction;

        SaveData save = JLoad(path);
        AsyncOperation asyncUnload = new AsyncOperation();
        AsyncOperation asyncLoad;
        asyncLoad = SceneManager.LoadSceneAsync(save.room,LoadSceneMode.Additive);
        if (!firstload) 
        {
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
        
        asyncLoad.allowSceneActivation = true;
        yield return new WaitForSecondsRealtime(0.1f);
        currentScene = SceneManager.GetSceneByBuildIndex(save.room);
        SceneManager.SetActiveScene(currentScene);
        InstantiateAll(ref save);

        saveAction.performed += SaveAction;
        loadAction.performed += LoadAction;
        pauseAction.performed += PauseAction;
    }

    private void InstantiateAll(ref SaveData save)
    {
        enemyManager = GameObject.Find("EnemyManager").GetComponent<EnemyManager>();
        playerEssentialsInstance = Instantiate(playerEssentials, Vector3.zero, Quaternion.identity);
        enemyManager.DeleteAllEnemies();
        try
        {
            enemyManager.Populate(currentSave.rooms[currentSave.room].enemies);
        }
        catch (System.Exception)
        {

        }

        playerController = playerEssentialsInstance.transform.Find("PlayerV5").GetComponent<PlayerController>();
        var charcon = playerController.GetComponent<CharacterController>();
        playerController.GetComponent<PlayerHealth>().gameManager = this;
        //var inventory = playerController.GetComponent<Inventory>();
        charcon.enabled = false;
        playerController.transform.position = SurrogateToVector(currentSave.player.position);
        playerController.transform.rotation = SurrogateToQuaternion(currentSave.player.rotation);
        charcon.enabled = true;
        playerController.isCrouching = currentSave.player.isCrouching;
        if (playerController.isCrouching)
        {
            playerController.GetComponent<Animator>().SetBool("CROUCHING", true);
        }
        else
        {
            playerController.GetComponent<Animator>().SetBool("CROUCHING", false);
        }
        playerController.SetMode(currentSave.player.mode);
        playerController.GetComponent<Health>().health = currentSave.player.health;
        playerController.GetComponent<Inventory>();//.ammo = (int[,])currentSave.player.ammo.Clone();
        enemyManager.currentAlertTime = 0;
        enemyManager.labelManager = playerEssentialsInstance.transform.Find("UIDocument").GetComponent<LabelManager>();
    }

    public void Transition(int room, Vector3 PlayerPos)
    {
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
        quit = true;
        SceneManager.LoadScene(0,LoadSceneMode.Single);
    }

    public void LoadAction(InputAction.CallbackContext context)
    {
        StartCoroutine(LoadSaveFile("currentsave.json"));
    }

    private void Load()
    {
        Debug.Log("Loading...");
        if (!firstload)
        {
            currentSave = JLoad("currentsave.json");
            SceneManager.UnloadSceneAsync(currentScene);
        }
        else
        {
            firstload = false;
            SceneManager.LoadScene(currentSave.room, LoadSceneMode.Additive);
        }
        
    }

    public void Save()
    {
        var playerdata = new PlayerData(VectorToSurrogate(playerController.transform.position), QuaternionToSurrogate(playerController.transform.rotation), playerController.mode, playerController.isCrouching, (int)playerController.GetComponent<Health>().health, playerController.GetComponent<Inventory>().ammo, 0);
        var room = SceneManager.GetActiveScene().buildIndex;
        var roomdata = (RoomData[])currentSave.rooms.Clone();
        roomdata[room].enemies = new EnemyData[enemyManager.enemies.Count];
        for (int i = 0; i < enemyManager.enemies.Count; i++)
        {
            var enemy = enemyManager.enemies[i];
            var enemyStateController = enemy.GetComponent<EnemyStateController>();
            roomdata[room].enemies[i] = new EnemyData(VectorToSurrogate(enemy.transform.position), QuaternionToSurrogate(enemy.transform.rotation), enemyStateController.state, InterruptSurrogate.FromInterrupt(enemy.interrupt), (int)enemy.GetComponent<Health>().health, (int)enemyStateController.stamina, enemyStateController.knockouttimer, enemy.nextPoint, VectorToSurrogateArray(enemy.patrolPoints));
        }
        currentSave = new SaveData(playerdata, roomdata, room);
        Debug.Log("SAVED");
        JSave("currentsave.json", currentSave);
        currentSave = JLoad("currentsave.json");
    }

    private void SaveAction(InputAction.CallbackContext context)
    {
        Save(); 
    }

    public void JSave(string path,SaveData data)
    {
        File.WriteAllText(path, JsonUtility.ToJson(data));
    }

    public SaveData JLoad(string path)
    {
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
    }
}
