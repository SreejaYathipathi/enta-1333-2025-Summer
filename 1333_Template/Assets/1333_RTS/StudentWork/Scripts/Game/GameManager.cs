using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TerrainUtils;


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
    }

    private void OnLoadGameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnLoadGameSceneLoaded;

        StartCoroutine(ShowLoadingThenInit());

        LoadPlayerSceneData();
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
        yield return null;
        SetState(GameState.Gameplay);
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
        data.ruby = ResourceManager.Instance.GetRuby();

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
        ResourceManager.Instance.SetRuby(data.ruby);

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