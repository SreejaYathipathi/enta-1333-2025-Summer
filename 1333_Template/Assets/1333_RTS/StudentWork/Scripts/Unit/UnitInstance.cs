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
            return;

        Vector3 nextWaypoint = _currentPath[_pathIndex].WorldPosition;
        Vector3 direction = (nextWaypoint - transform.position).normalized;
        float step = _moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, nextWaypoint, step);

        if (Vector3.Distance(transform.position, nextWaypoint) < 0.05f)
        {
            _pathIndex++;
            if (_pathIndex >= _currentPath.Count)
            {
                _isMoving = false;
            }
        }
    }

    public void SetTarget(Vector3 worldPosition)
    {
        _targetWorldPosition = worldPosition;
        _currentPath = _pathfinder.Findpath(transform.position, worldPosition);
        _pathIndex = 0;
        _isMoving = _currentPath != null && _currentPath.Count > 1;
    }

    public void SetTarget(GridNode node)
    {
        SetTarget(node.WorldPosition);
    }

    public override void MoveTo(GridNode targetNode)
    {
        SetTarget(targetNode);
    }
}
