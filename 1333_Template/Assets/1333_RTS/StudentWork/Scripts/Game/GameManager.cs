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
    Paused,
    GameOver
}

//[ExecuteAlways]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState CurrentState { get; private set; }

    public event Action<GameState> OnStateChanged;

    [Header("Scene References")]
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private UnitManager _unitManager;

    [Header("Loading Settings")]
    [SerializeField] private float loadingScreenDuration = 1f;

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
    }

    private IEnumerator DelayedSetInitialState()
    {
        yield return null; // wait one frame
        SetState(GameState.MainMenu);
    }

    // Change game state and notify listeners
    public void SetState(GameState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
        Debug.Log($"[GameManager] State changed to {newState}");
    }

    // Called when clicking "Start Game" from Main Menu
    public void StartGame()
    {
        SetState(GameState.Loading);
        SceneManager.sceneLoaded += OnGameplaySceneLoaded;
        SceneManager.LoadScene("PlayerScene");  // your gameplay scene name
    }

    private void OnGameplaySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnGameplaySceneLoaded;
        StartCoroutine(ShowLoadingThenInit());
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