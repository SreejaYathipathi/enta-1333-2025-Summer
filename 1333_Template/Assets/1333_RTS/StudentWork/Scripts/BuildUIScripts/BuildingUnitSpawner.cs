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
    [SerializeField] private BuildItemData _buildItemData;

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
                SpawnUnits(spawnCount);
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
        float nodeSize = _gridManager.GridSettings.NodeSize;

        GridNode centerNode = _gridManager.GetNodeFromWorldPosition(transform.position);
        if (centerNode == null)
        {
            Debug.LogWarning("[Spawner] Building not on valid grid.");
            return;
        }

        Vector3 rightDir = transform.right.normalized;
        Vector3 forwardDir = transform.forward.normalized;

        Vector3 frontCenter = transform.position + forwardDir * ((_buildItemData.footprintSize.y / 2f) * nodeSize + nodeSize * 0.5f);

        //Vector3 offsetFromBuilding = forwardDir * (_buildItemData.footprintSize.y / 2f *  nodesize + nodesize * 0.5f);

        //Vector3 origin = transform.position + offsetFromBuilding;

        /*float pushforward = _gridManager.GridSettings.NodeSize * (maxRows + 0.5f);
        //Vector3 origin = transform.position + transform.forward;
        Vector3 origin = transform.position + forwardDir * pushforward;*/

        //Vector3 origin = transform.position + spawnDirection.normalized;

        for (int i = 0; i < count; i++)
        {

            Vector3 offset = rightDir * (col * spacing) + forwardDir * (row * spacing);
            Vector3 spawnPos = frontCenter + offset;

            //float nodeHeight = _gridManager.GridSettings.NodeSize;
            //Vector3 liftedSpawnPos = new Vector3(spawnPos.x, nodeHeight / 2.5f + 0.1f, spawnPos.z);

            GridNode node = _gridManager.GetNodeFromWorldPosition(spawnPos);
            if (node != null && node.Walkable && !node.IsOccupied)
            {
                float yHeight = nodeSize / 2.5f + 0.1f;
                Vector3 liftedSpawnPos = new Vector3(node.WorldPosition.x, yHeight, node.WorldPosition.z);

                GameObject newUnit = Instantiate(unitPrefab, liftedSpawnPos, Quaternion.identity);
                newUnit.transform.SetParent(transform);
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
