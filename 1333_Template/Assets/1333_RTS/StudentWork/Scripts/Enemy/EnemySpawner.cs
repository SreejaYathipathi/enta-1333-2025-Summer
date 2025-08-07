using System.Collections;
using System.Linq;
using UnityEngine;

/// Spawns enemy waves and handles wave flow / victory conditions.
public class EnemySpawner : MonoBehaviour
{
    public AStarPathFinding pathfinder;
    public GridManager gridManager;

    [Header("Enemy Composition")]
    public ArmyComposition enemyUnits;

    [Header("Spawn Settings")]
    public float spawnInterval = 5f;
    public int minEnemiesPerWave = 1;
    public int maxEnemiesPerWave = 3;

    public static bool WaveActive { get; private set; } = false;

    [Header("UI")]
    public GameObject wavePanel;

    private void Start()
    {
        StartCoroutine(SpawnEnemiesRoutine());
    }

    private void Update()
    {
        if (WaveActive && AreAllBuildingsDestroyed())
        {
            WaveActive = false;
            GameManager.Instance.GameOver(false);
        }
    }

    private void Awake()
    {
        if (pathfinder == null)
        {
            var grid = FindObjectOfType<GridManager>();
            pathfinder = new AStarPathFinding(grid);
        }
    }

    // Main loop: waits, shows panel, spawns enemies, waits for clear.
    private IEnumerator SpawnEnemiesRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval - 5f);

            if (GameObject.FindObjectsOfType<BuildingHealth>().Length == 0)
            {
                Debug.Log("No buildings placed — skipping enemy wave.");
                yield return new WaitForSeconds(5f);
                continue;
            }

            yield return StartCoroutine(ShowWavePanel());

            // **Wave starts**
            WaveActive = true;
            GameManager.Instance.IncreaseWave();
            SetPlayerUnitsControlMode(ControlMode.AI);

            int spawnCount = Random.Range(minEnemiesPerWave, maxEnemiesPerWave + 1);

            for (int i = 0; i < spawnCount; i++)
            {
                var entries = enemyUnits.units;
                var randomEntry = entries[Random.Range(0, entries.Count)];
                if (randomEntry.unitTypePrefab == null || randomEntry.unitTypePrefab.prefab == null)
                    continue;

                Vector3 spawnPos = GetRandomEdgePosition();
                Vector3 lifted = spawnPos + Vector3.up * (gridManager.GridSettings.NodeSize / 2.5f + 0.1f);

                GameObject go = Instantiate(randomEntry.unitTypePrefab.prefab, lifted, Quaternion.identity);
                UnitInstance unit = go.GetComponent<UnitInstance>();
                unit.Initialize(pathfinder, randomEntry.unitTypePrefab.unitType);
                unit.SetArmy(1);
                unit.SetControlMode(ControlMode.AI);

                Debug.Log($"[EnemySpawner] Spawned: {unit.name}");
            }

            Debug.Log($"[EnemySpawner] Spawned {spawnCount} enemies this wave.");

            // Wait until all enemies are dead
            yield return new WaitUntil(() => GameObject.FindObjectsOfType<UnitInstance>().All(u => u.ArmyID != 1));

            // **Wave ends**
            WaveActive = false;
            SetPlayerUnitsControlMode(ControlMode.Manual);

            if (AreAllBuildingsDestroyed())
            {
                GameManager.Instance.GameOver(false);

            }
            else
            {
                XPManager.Instance.AddXP(30 + 10 * GameManager.Instance.CurrentWave);
                GameManager.Instance.GameOver(true);
            }

            foreach (var unit in FindObjectsOfType<UnitHealth>())
                unit.ResetHealth();

            foreach (var building in FindObjectsOfType<BuildingHealth>())
                building.ResetHealth();

            Debug.Log("[EnemySpawner] All enemies cleared. Preparing next wave...");
        }
    }

    // True if player has no buildings left.
    private bool AreAllBuildingsDestroyed()
    {
        return GameObject.FindObjectsOfType<BuildingHealth>().Length == 0;
    }

    // Shows “Next wave” panel and waits for click.
    private IEnumerator ShowWavePanel()
    {
        wavePanel.SetActive(true);
        bool clicked = false;

        void OnClick() => clicked = true;

        var button = wavePanel.GetComponentInChildren<UnityEngine.UI.Button>();
        button.onClick.AddListener(OnClick);

        yield return new WaitUntil(() => clicked);

        button.onClick.RemoveListener(OnClick);
        wavePanel.SetActive(false);
    }

    // Picks a random grid edge cell for spawning.
    private Vector3 GetRandomEdgePosition()
    {
        int edge = Random.Range(0, 4);

        int gridX = gridManager.GridSettings.GridSizeX;
        int gridY = gridManager.GridSettings.GridSizeY;

        int x = 0, y = 0;

        switch (edge)
        {
            case 0: // Left
                x = 0;
                y = Random.Range(0, gridY);
                break;
            case 1: // Right
                x = gridX - 1;
                y = Random.Range(0, gridY);
                break;
            case 2: // Top
                x = Random.Range(0, gridX);
                y = gridY - 1;
                break;
            case 3: // Bottom
                x = Random.Range(0, gridX);
                y = 0;
                break;
        }

        return gridManager.GetNode(x, y).WorldPosition;
    }

    // Switches all player units between Manual and AI modes.
    private void SetPlayerUnitsControlMode(ControlMode mode)
    {
        var armyTester = FindObjectOfType<ArmyPathFindingTester>();
        if (armyTester != null && armyTester.PlayerArmy != null)
        {
            foreach (var unit in armyTester.PlayerArmy.Units)
            {
                var instance = unit as UnitInstance;
                if (instance != null)
                    instance.SetControlMode(mode);
            }
        }
    }

}
