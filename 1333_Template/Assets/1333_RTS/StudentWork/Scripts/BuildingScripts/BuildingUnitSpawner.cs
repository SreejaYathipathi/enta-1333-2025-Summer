using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Spawns a unit prefab at a designated point when the player presses F.
public class BuildingUnitSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject unitPrefab;
    public Transform spawnPoint;
    [SerializeField] private int spawnCount = 1;

    private void Start()
    {
        // Auto-search for a child named “UnitSpawnPoint” if none set.
        if (spawnPoint == null)
        {
            spawnPoint = transform.Find("UnitSpawnPoint");
            if (spawnPoint == null)
                Debug.LogError("[Spawner] No spawn point found!");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            TrySpawn();
        }
    }

    /// Instantiates the unit if the grid node is walkable and free.
    private void TrySpawn()
    {
        if (unitPrefab == null || spawnPoint == null)
        {
            Debug.LogError("[Spawner] Missing references.");
            return;
        }

        GridManager grid = FindObjectOfType<GridManager>();
        GridNode node = grid.GetNodeFromWorldPosition(spawnPoint.position);

        if (node != null && node.Walkable && !node.IsOccupied)
        {
            GameObject unitGO = Instantiate(unitPrefab, spawnPoint.position, Quaternion.identity);
            node.IsOccupied = true;

            UnitInstance unit = unitGO.GetComponent<UnitInstance>();
            ArmyPathFindingTester tester = FindObjectOfType<ArmyPathFindingTester>();

            if (unit != null && tester != null)
            {
                unit.Initialize(tester.SharedPathfinder, unit.UnitType);
                unit.SetArmy(0); // Player army
                unit.ForceSetCurrentNode(node); // Set internal node reference

                tester.PlayerArmy.Units.Add(unit);
                tester.RegisterPatrollingUnit(unit); // Delay coroutine starts after setup
            }

            Debug.Log($"[Spawner] Spawned {unit.name} at {spawnPoint.position}");
        }
        else
        {
            Debug.LogWarning("[Spawner] Cannot spawn — node blocked or invalid.");
        }
    }
}
