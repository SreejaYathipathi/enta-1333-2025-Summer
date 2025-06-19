using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitInstance : UnitBase
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 3f; // Units per second.

    private AStarPathFinding _pathfinder; // Reference to the Pathfinder.
    private List<GridNode> _currentPath = new List<GridNode>(); // The current path to follow.
    private int _pathIndex = 0; // Current waypoint index.
    private Vector3? _targetWorldPosition = null; // The current target position.
    private bool _isMoving = false; // Is the unit currently moving?
    private GridNode _currentNode;

    public bool IsMoving => _isMoving;

    public List<GridNode> CurrentPath => _currentPath;

    public void Initialize(AStarPathFinding pathfinder, UnitType unitType)
    {
        _pathfinder = pathfinder;
        _unitType = unitType;
    }

    private void Update()
    {
        if (!_isMoving || _currentPath == null || _currentPath.Count == 0 || _pathIndex >= _currentPath.Count)
        {
            _isMoving = false;
            return;
        }

        GridNode nextNode = _currentPath[_pathIndex];

        if (!nextNode.Walkable || nextNode.IsOccupied)
        {
            Debug.LogWarning($"[Repath] {name} detected blocked node at {_pathIndex} ({nextNode.GridX},{nextNode.GridY}). Repathing...");
            if (_targetWorldPosition.HasValue)
            {
                TargetSet(_targetWorldPosition.Value);
            }
            return;
        }

        Vector3 nextWaypoint = new Vector3(nextNode.WorldPosition.x, transform.position.y, nextNode.WorldPosition.z);
        float step = _moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, nextWaypoint, step);

        GridNode nodeNow = _pathfinder.GridManager.GetNodeFromWorldPosition(transform.position);

        if (_currentNode != null && _currentNode != nodeNow)
        {
            _currentNode.IsOccupied = false; // Leave old node
        }
        if (nodeNow != null)
        {
            nodeNow.IsOccupied = true; // Occupy new node
            _currentNode = nodeNow;
        }

        if (Vector3.Distance(transform.position, nextWaypoint) < 0.05f)
        {
            _pathIndex++;
            if (_pathIndex >= _currentPath.Count)
            {
                _isMoving = false;
                _currentPath.Clear();
            }
        }
    }

    public bool HasReachedDestination()
    {
        return !_isMoving && _currentPath != null && _currentPath.Count > 0 && _pathIndex >= _currentPath.Count;
    }

    public void TargetSet(Vector3 worldPosition)
    {

        Debug.Log($"[SetTarget] {name} trying to move to {worldPosition}");

        if (_pathfinder == null)
        {
            Debug.LogError($"[SetTarget] Pathfinder is NULL for {name}");
            return;
        }

        //transform.position = _pathfinder.GridManager.GetNodeFromWorldPosition(transform.position).WorldPosition;

        _currentPath = _pathfinder.Findpath(transform.position, worldPosition);

        if (_currentPath == null)
        {
            Debug.LogError($"[SetTarget] {name} path is NULL.");
            _isMoving = false;
            _currentPath = new List<GridNode>();
            return;
        }

        if (_currentPath.Count <= 1)
        {
            Debug.LogWarning($"[SetTarget] {name} path too short. Count = {_currentPath.Count}");
            _isMoving = false;
            _currentPath.Clear();
            return;
        }

        _pathIndex = 0;

        if (!_currentPath[0].Walkable || _currentPath[0].IsOccupied)
        {
            Debug.LogWarning($"[SetTarget] {name} path starts on invalid node, canceling move.");
            _isMoving = false;
            _currentPath.Clear();
            return;
        }


        _targetWorldPosition = worldPosition;
        _isMoving = true;

        Debug.Log($"[SetTarget] {name} path assigned with {_currentPath.Count} nodes");

        for (int i = 0; i < _currentPath.Count - 1; i++)
        {
            Debug.DrawLine(
                _currentPath[i].WorldPosition + Vector3.up * 1f,
                _currentPath[i + 1].WorldPosition + Vector3.up * 1f,
                Color.cyan, 5f
            );
        }

    }

    public void SetTarget(GridNode node)
    {
        TargetSet(node.WorldPosition);
    }

    public override void MoveTo(GridNode targetNode)
    {
        SetTarget(targetNode);
    }

    private void OnDestroy()
    {
        if (_currentNode != null)
        {
            _currentNode.IsOccupied = false;
        }
    }
}
