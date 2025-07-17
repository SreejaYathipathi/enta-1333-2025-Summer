using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingUnitSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject unitPrefab;
    public Transform spawnPoint;
    [SerializeField] private int spawnCount = 1;

    private void Start()
    {
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
            GameObject unit = Instantiate(unitPrefab, spawnPoint.position, Quaternion.identity);
            node.IsOccupied = true;
            Debug.Log($"[Spawner] Spawned {unit.name} at {spawnPoint.position}");
        }
        else
        {
            Debug.LogWarning("[Spawner] Cannot spawn — node blocked or invalid.");
        }
    }
}
