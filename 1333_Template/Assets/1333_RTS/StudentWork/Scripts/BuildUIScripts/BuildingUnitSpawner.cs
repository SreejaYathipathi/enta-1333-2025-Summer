using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingUnitSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject unitPrefab;
    public int unitsPerRow = 5;
    public int maxRows = 3;
    public float spacing = 1.5f;
    [SerializeField] private int spawnCount = 12;
    //public Vector3 spawnDirection = Vector3.forward;

    [Header("Input Settings")]
    public KeyCode spawnKey = KeyCode.F;

    private GridManager _gridManager;

    //Track spawned units
    public List<GameObject> spawnedUnits = new List<GameObject>();

    private void Start()
    {
        _gridManager = FindObjectOfType<GridManager>();

        if (_gridManager == null)
        {
            Debug.LogError("[BuildingUnitSpawner] No GridManager found in scene.");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(spawnKey))
        {
            if (IsOnValidGrid())
            {
                SpawnUnits(12);
            }
            else
            {
                Debug.LogWarning("[Spawner] Cannot spawn: building not on valid grid.");
            }

            // UI Button version (for later):
        }
    }

    private bool IsOnValidGrid()
    {
        GridNode node = _gridManager.GetNodeFromWorldPosition(transform.position);
        return node != null && node.Walkable && !node.IsOccupied;
    }

    public void SpawnUnits(int count)
    {
        if (unitPrefab == null || _gridManager == null)
        {
            Debug.LogError("[Spawner] Missing references.");
            return;
        }

        int row = 0, col = 0;
        Vector3 origin = transform.position + transform.forward;

        //Vector3 origin = transform.position + spawnDirection.normalized;

        for (int i = 0; i < count; i++)
        {

            Vector3 rightDir = transform.right;
            Vector3 forwardDir = transform.forward;
            Vector3 offset = (rightDir * col * spacing) + (forwardDir * row * spacing);
            Vector3 spawnPos = origin + offset;

            GridNode node = _gridManager.GetNodeFromWorldPosition(spawnPos);
            if (node != null && node.Walkable && !node.IsOccupied)
            {
                GameObject newUnit = Instantiate(unitPrefab, spawnPos, Quaternion.identity);

                // Parent to this building
                newUnit.transform.SetParent(transform);

                // Add to tracking list
                spawnedUnits.Add(newUnit);

                node.IsOccupied = true;
            }
            else
            {
                Debug.LogWarning($"[Spawner] Cannot spawn at {spawnPos} — blocked or invalid.");
            }

            col++;
            if (col >= unitsPerRow)
            {
                col = 0;
                row++;
                if (row >= maxRows) break;
            }
        }
    }
}
