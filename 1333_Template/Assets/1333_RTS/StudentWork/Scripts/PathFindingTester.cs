using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class PathFindingTester : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Vector2Int startWorldPos;
    [SerializeField] private Vector2Int endWorldPos;

    private AStarPathFinding pathFinder;

    private void Start()
    {
        pathFinder = new AStarPathFinding(gridManager);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Starting A* pathfinding test...");

            // Convert world positions to grid nodes
            GridNode startNode = gridManager.GetNode(startWorldPos.x, startWorldPos.y);
            GridNode endNode = gridManager.GetNode(endWorldPos.x, endWorldPos.y);

            Debug.Log(startNode.WorldPosition);
            Debug.Log(endNode.WorldPosition);

            // Clear previous debug data
            gridManager.Path.Clear();
            gridManager.Visited.Clear();
            gridManager.Frontier.Clear();

            // Find and store path
            List<GridNode> path = pathFinder.Findpath(startNode, endNode);

            if (path != null)
            {
                Debug.Log($"Path found with {path.Count} nodes.");
                gridManager.Path = path;
            }
            else
            {
                Debug.LogWarning("No path could be found.");
            }
        }
    }
}
