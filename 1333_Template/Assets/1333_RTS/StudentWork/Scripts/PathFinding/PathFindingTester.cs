using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PathFindingTester : MonoBehaviour
{
    [SerializeField] private GridManager _gridManager;

    private AStarPathFinding _pathfinder;
    private LineRenderer _lineRenderer;

    private GridNode _startNode;
    private GridNode _endNode;

    void Start()
    {
        _pathfinder = new AStarPathFinding(_gridManager);
        _lineRenderer = GetComponent<LineRenderer>();

        _lineRenderer.positionCount = 0;
        _lineRenderer.startWidth = 0.2f;
        _lineRenderer.endWidth = 0.2f;
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.startColor = Color.cyan;
        _lineRenderer.endColor = Color.cyan;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            _gridManager.InitializeGrid();
            _lineRenderer.positionCount = 0;
            Debug.Log("Grid regenerated.");
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (_gridManager.IsInitialized)
            {
                PickRandomStartEnd();
                List<GridNode> path = _pathfinder.Findpath(_startNode, _endNode);
                DrawPath(path);
                Debug.Log($"Pathfinding from {_startNode.Name} to {_endNode.Name}");
            }
        }
    }

    void PickRandomStartEnd()
    {
        List<GridNode> walkables = new List<GridNode>();
        for (int x = 0; x < _gridManager.GridSettings.GridSizeX; x++)
        {
            for (int y = 0; y < _gridManager.GridSettings.GridSizeY; y++)
            {
                GridNode node = _gridManager.GetNode(x, y);
                if (node != null && node.Walkable)
                    walkables.Add(node);
            }
        }

        if (walkables.Count < 2)
        {
            Debug.LogWarning("Not enough walkable nodes to select start and end.");
            return;
        }

        _startNode = walkables[Random.Range(0, walkables.Count)];
        _endNode = walkables[Random.Range(0, walkables.Count)];

        while (_endNode == _startNode)
        {
            _endNode = walkables[Random.Range(0, walkables.Count)];
        }
    }

    private void DrawPath(List<GridNode> path)
    {
        if (path == null || path.Count == 0)
        {
            _lineRenderer.positionCount = 0;
            Debug.Log("No path found.");
            return;
        }

        _lineRenderer.positionCount = path.Count;
        int totalCost = 0;

        for (int i = 0; i < path.Count; i++)
        {
            _lineRenderer.SetPosition(i, path[i].WorldPosition + Vector3.up * 0.1f);
            totalCost += path[i].Weight;
        }

        Debug.Log($"Path length: {path.Count}, Total movement cost: {totalCost}");
    }
}
