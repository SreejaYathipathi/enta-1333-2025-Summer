using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TerrainUtils;
using UnityEngine.UIElements;


public enum GameState
{
    MainMenu,
    Loading,
    Gameplay,
    EnemyBattle,
    Paused,
    GameOver
}

//[ExecuteAlways]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState CurrentState { get; private set; }

    public event Action<GameState> OnStateChanged;

    public int CurrentWave { get; private set; } = 1;
    private int currentSlot;

    private DateTime lastSaveTime = DateTime.MinValue;

    [Header("Scene References")]
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private UnitManager _unitManager;

    [Header("Loading Settings")]
    [SerializeField] private float loadingScreenDuration = 1f;

    [Header("Prefab Database")]
    public PrefabDatabase prefabDatabase;

    [Header("Starting Resources")]                      // NEW
    [SerializeField] private int startWood = 200;
    [SerializeField] private int startStone = 100;
    [SerializeField] private int startCrystal = 0;
    [SerializeField] private int startAqua = 0;
    [SerializeField] private int startAmethyst = 0;
    [SerializeField] private int startEmerald = 0;

    private readonly Dictionary<ResourceType, int> _pendingRewards = new Dictionary<ResourceType, int>();

    private bool giveStartingResources = false;

    private bool _pendingLoad = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        StartCoroutine(DelayedSetInitialState());
        currentSlot = PlayerPrefs.GetInt("LastUsedSlot", 0);
    }

    private IEnumerator DelayedSetInitialState()
    {
        yield return null;
        SetState(GameState.MainMenu);
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
        Debug.Log($"[GameManager] State changed to {newState}");
    }

    public void StartNewGame()
    {
        // Clear any previous saved data for this slot
        PlayerPrefs.DeleteKey($"PlayerSceneSave_{currentSlot}");

        giveStartingResources = true;

        SceneManager.sceneLoaded += OnGameplaySceneLoaded;
        SceneManager.LoadScene("PlayerScene");

        SetState(GameState.Loading);
    }

    // Loads an existing saved game
    public void LoadGame()
    {
        SceneManager.sceneLoaded += OnLoadGameSceneLoaded;
        SceneManager.LoadScene("PlayerScene");
        SetState(GameState.Loading);
    }

    private void OnGameplaySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnGameplaySceneLoaded;
        StartCoroutine(ShowLoadingThenInit());

        if (giveStartingResources)
        {
            ResourceManager res = ResourceManager.Instance;
            res.SetWood(startWood);
            res.SetStone(startStone);
            res.SetCrystal(startCrystal);
            res.SetAqua(startAqua);
            res.SetAmethyst(startAmethyst);
            res.SetEmerald(startEmerald);

            giveStartingResources = false;
        }
    }

    private void OnLoadGameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnLoadGameSceneLoaded;

        _pendingLoad = true;

        StartCoroutine(ShowLoadingThenInit());

        StartCoroutine(LoadPlayerSceneRoutine());
    }

    private void OnEnemySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnEnemySceneLoaded;

        _gridManager = FindObjectOfType<GridManager>();
        if (_gridManager != null)
            _gridManager.InitializeGrid();

        var loader = FindObjectOfType<EnemyBaseLoader>();
        if (loader != null)
            loader.enabled = true;

        SetState(GameState.EnemyBattle);
    }

    public void StartEnemyBattle()
    {
        SavePlayerSceneData();
        SetState(GameState.Loading);
        SceneManager.sceneLoaded += OnEnemySceneLoaded;
        SceneManager.LoadScene("EnemyScene");
    }

    public void EndEnemyBattle()
    {
        Time.timeScale = 1f;

        // Go back to PlayerScene
        SceneManager.sceneLoaded += OnLoadGameSceneLoaded;
        SceneManager.LoadScene("PlayerScene");

        // Switch state
        SetState(GameState.Loading);
    }

    private IEnumerator ShowLoadingThenInit()
    {
        SetState(GameState.Loading);
        yield return new WaitForSeconds(loadingScreenDuration);

        InitializeGameplaySystems();

        if (_pendingLoad)
        {
            LoadPlayerSceneData();
            _pendingLoad = false;
        }

        yield return null;
        SetState(GameState.Gameplay);
    }

    public void AddPendingReward(ResourceType type, int amount)
    {
        if (_pendingRewards.ContainsKey(type))
            _pendingRewards[type] += amount;
        else
            _pendingRewards[type] = amount;
    }

    private void ApplyPendingRewards()
    {
        foreach (var kv in _pendingRewards)
            ResourceManager.Instance.AddResource(kv.Key, kv.Value);

        _pendingRewards.Clear();
    }

    private IEnumerator LoadPlayerSceneRoutine()
    {
        yield return StartCoroutine(ShowLoadingThenInit()); // grid / units

        yield return null;          // ensure every singleton has run Awake()

        LoadPlayerSceneData();      // restore normal save
        ApplyPendingRewards();      // now add the loot
    }

    private void InitializeGameplaySystems()
    {
        _gridManager = FindObjectOfType<GridManager>();
        if (_gridManager != null)
        {
            _gridManager.InitializeGrid(); // always rebuild grid
            Debug.Log("[GameManager] Grid initialized.");
        }
        else
        {
            Debug.LogWarning("[GameManager] No GridManager found in scene.");
        }

        _unitManager = FindObjectOfType<UnitManager>();
        if (_unitManager != null)
        {
            _unitManager.SpawnDummyUnit(transform);
            Debug.Log("[GameManager] Units ready.");
        }
        else
        {
            Debug.LogWarning("[GameManager] No UnitManager found in scene.");
        }

        var enemySpawner = FindObjectOfType<EnemySpawner>();
        if (enemySpawner != null)
        {
            Debug.Log("[GameManager] Enemy spawner is ready.");
        }
    }

    public void IncreaseWave()
    {
        CurrentWave++;
        UIManager.Instance.UpdateWaveText(CurrentWave);
    }

    public void SavePlayerSceneData()
    {
        PlayerSceneData data = new PlayerSceneData();

        // Resources
        data.wood = ResourceManager.Instance.GetWood();
        data.stone = ResourceManager.Instance.GetStone();
        data.crystal = ResourceManager.Instance.GetCrystal();
        data.aqua = ResourceManager.Instance.GetAqua();
        data.amethyst = ResourceManager.Instance.GetAmethyst();
        data.emerald = ResourceManager.Instance.GetEmerald();

        data.xp = XPManager.Instance.CurrentXP;
        data.level = XPManager.Instance.CurrentLevel;

        // Buildings
        foreach (var b in FindObjectsOfType<BuildingHealth>())
        {
            BuildingData bd = new BuildingData();
            bd.prefabName = b.name.Replace("(Clone)", "");
            bd.posX = b.transform.position.x;
            bd.posY = b.transform.position.y;
            bd.posZ = b.transform.position.z;
            bd.rotY = b.transform.rotation.eulerAngles.y;
            data.buildings.Add(bd);
        }

        var obstacles = FindObjectsOfType<ObstacleCuttable>();
        foreach (var obs in obstacles)
        {
            MapObstacleData od = new MapObstacleData
            {
                prefabName = obs.name.Replace("(Clone)", ""),
                posX = obs.transform.position.x,
                posY = obs.transform.position.y,
                posZ = obs.transform.position.z,
                rotY = obs.transform.rotation.eulerAngles.y
            };
            data.mapData.obstacles.Add(od);
        }

        // Progress
        data.completedWaves = CurrentWave;

        lastSaveTime = DateTime.Now;

        SaveManager.SavePlayerScene(data, currentSlot);
    }

    public void LoadPlayerSceneData()
    {
        PlayerSceneData data = SaveManager.LoadPlayerScene(currentSlot);
        if (data == null)
        {
            Debug.Log("No PlayerScene save found");
            return;
        }

        // Resources
        ResourceManager.Instance.SetWood(data.wood);
        ResourceManager.Instance.SetStone(data.stone);
        ResourceManager.Instance.SetCrystal(data.crystal);
        ResourceManager.Instance.SetAqua(data.aqua);
        ResourceManager.Instance.SetAmethyst(data.amethyst);
        ResourceManager.Instance.SetEmerald(data.emerald);

        XPManager.Instance.SetXPAndLevel(data.xp, data.level);

        // Buildings
        foreach (var b in data.buildings)
        {
            GameObject prefab = prefabDatabase.GetPrefabByName(b.prefabName);
            if (prefab == null)
            {
                Debug.LogError($"[SaveLoad] Prefab not found for {b.prefabName}");
                continue;
            }

            Vector3 pos = new Vector3(b.posX, b.posY, b.posZ);
            BuildingPlacer.Instance.PlaceBuildingFromSave(prefab, pos, b.rotY);
        }

        // Obstacles
        foreach (var obs in data.mapData.obstacles)
        {
            GameObject prefab = prefabDatabase.GetPrefabByName(obs.prefabName);
            if (prefab == null)
            {
                Debug.LogError($"Obstacle prefab not found: {obs.prefabName}");
                continue;
            }
            Vector3 pos = new Vector3(obs.posX, obs.posY, obs.posZ);
            Quaternion rot = Quaternion.Euler(0, obs.rotY, 0);
            Instantiate(prefab, pos, rot);
        }

        CurrentWave = data.completedWaves;
    }



    public void PauseGame()
    {
        Time.timeScale = 0f;
        SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        SetState(GameState.Gameplay);
    }

    public void GameOver(bool won)
    {
        Time.timeScale = 0f;
        SetState(GameState.GameOver);

        foreach (var unit in FindObjectsOfType<UnitInstance>())
        {
            unit.StopAllCoroutines();
        }
        foreach (var tester in FindObjectsOfType<ArmyPathFindingTester>())
        {
            tester.StopAllCoroutines();
        }

        // Notify UI which result to show
        UIManager.Instance.ShowGameOver(won);
    }

    private void OnApplicationQuit()
    {
        SavePlayerSceneData();
    }

    public void SaveGameButton()
    {
        SavePlayerSceneData();
        lastSaveTime = DateTime.Now;
        Debug.Log("Game saved manually via button");
    }

    public string GetTimeSinceLastSave()
    {
        if (lastSaveTime == DateTime.MinValue)
            return "Never";

        TimeSpan elapsed = DateTime.Now - lastSaveTime;
        if (elapsed.TotalSeconds < 60)
            return $"{elapsed.Seconds} seconds ago";
        else
            return $"{(int)elapsed.TotalMinutes} minutes ago";
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.sceneLoaded += OnGameplaySceneLoaded;
        SceneManager.LoadScene("PlayerScene");
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
        SetState(GameState.MainMenu);
    }

}