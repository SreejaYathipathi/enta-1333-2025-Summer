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

    private UnitInstance targetUnit;
    private BuildingHealth targetBuilding;

    private float _detectionRange = 3f;

    public UnitInstance GetTargetUnit() => targetUnit;
    public BuildingHealth GetTargetBuilding() => targetBuilding;

    public bool HasTargetUnit() => targetUnit != null;
    public bool HasTargetBuilding() => targetBuilding != null;

    public void ClearTargetUnit() => targetUnit = null;
    public void ClearTargetBuilding() => targetBuilding = null;

    public UnitType UnitType => _unitType;

    public int Damage => _unitType.damage;
    public int Range => _unitType.range;

    private bool _hasEvaluatedInitialTarget = false;

    public void MoveToPosition(Vector3 pos) => TargetSet(pos);

    public bool IsMoving => _isMoving;

    public List<GridNode> CurrentPath => _currentPath;

    public void Initialize(AStarPathFinding pathfinder, UnitType unitType)
    {
        _pathfinder = pathfinder;
        _unitType = unitType;
    }

    private void Update()
    {

        if (!_isMoving && !_hasEvaluatedInitialTarget)
        {
            EvaluateTarget();
            _hasEvaluatedInitialTarget = true;
        }

        Debug.Log($"[{name}] _isMoving: {_isMoving}, HasTargetUnit: {HasTargetUnit()}, HasTargetBuilding: {HasTargetBuilding()}, HasEvaluated: {_hasEvaluatedInitialTarget}");

        if (!_isMoving || _currentPath == null || _currentPath.Count == 0 || _pathIndex >= _currentPath.Count)
        {
            _isMoving = false;
            return;
        }

        GridNode nextNode = _currentPath[_pathIndex];

        bool isBlockedByOtherUnit = nextNode.IsOccupied && nextNode != _currentNode;

        if (!nextNode.Walkable || isBlockedByOtherUnit)
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

                _pathfinder.GridManager.Visited.Clear();
                _pathfinder.GridManager.Frontier.Clear();
            }
        }

        /* if (!_isMoving && targetUnit == null && targetBuilding == null)
         {
             EvaluateTarget();
         }*/
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

        GridNode startNode = _pathfinder.GridManager.GetNodeFromWorldPosition(transform.position);
        GridNode endNode = _pathfinder.GridManager.GetNodeFromWorldPosition(worldPosition);


        if (startNode == null || !startNode.Walkable || (startNode.IsOccupied && startNode != _currentNode))
        {
            Debug.LogWarning($"[SetTarget] {name} is standing on an invalid starting node. Cannot path.");
            _isMoving = false;
            _currentPath = new List<GridNode>();
            return;
        }

        if (endNode == null || !endNode.Walkable)
        {
            Debug.LogWarning($"[SetTarget] {name} cannot reach unwalkable target node.");
            _isMoving = false;
            _currentPath = new List<GridNode>();
            return;
        }

        _currentPath = _pathfinder.Findpath(startNode, endNode);


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

        if (_currentPath[0] != startNode)
        {
            Debug.LogWarning($"[SetTarget] {name} path starts incorrectly. Canceling move.");
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

    /*public void EvaluateTarget()
    {
        // 1. Look for nearby enemy units first
        UnitInstance closestEnemy = FindNearestEnemyInRange();
        if (closestEnemy != null)
        {
            targetUnit = closestEnemy;
            return;
        }

        // 2. Look for buildings
        BuildingHealth bestBuilding = FindBestBuildingTarget();
        if (bestBuilding != null)
        {
            targetBuilding = bestBuilding;

            GridNode nearNode = GetNearbyValidNode(bestBuilding.transform.position);
            if (nearNode != null)
                TargetSet(nearNode.WorldPosition);
            else
                Debug.LogWarning($"[Targeting] No nearby walkable node for {bestBuilding.name}");
        }
    }*/

    public void EvaluateTarget()
    {
        Debug.Log($"[{name}] Evaluating target...");

        UnitInstance closestEnemy = FindNearestEnemyInRange();
        if (closestEnemy != null)
        {
            targetUnit = closestEnemy;
            Debug.Log($"[{name}] Found enemy unit: {closestEnemy.name}");
            return;
        }

        BuildingHealth bestBuilding = FindBestBuildingTarget();
        if (bestBuilding != null)
        {
            Debug.Log($"[{name}] Found building: {bestBuilding.name}");
            targetBuilding = bestBuilding;

            Vector2Int footprint = bestBuilding.FootprintSize;
            Debug.Log($"[{name}] Target building footprint: {footprint.x} x {footprint.y}");
            int radius = Mathf.CeilToInt(Mathf.Max(footprint.x, footprint.y) / 2f);

            GridNode nearNode = GetNearbyValidNode(bestBuilding.transform.position, radius);
            if (nearNode != null)
            {
                Debug.Log($"[{name}] Moving to nearby node {nearNode.GridX},{nearNode.GridY}");
                TargetSet(nearNode.WorldPosition);
            }
            else
            {
                Debug.LogWarning($"[{name}] No valid node near {bestBuilding.name}");
            }
        }

        else
        {
            Debug.Log($"[{name}] No building found.");
        }
    }

    private GridNode GetNearbyValidNode(Vector3 center, int radius)
    {
        GridNode centerNode = _pathfinder.GridManager.GetNodeFromWorldPosition(center);
        if (centerNode == null) return null;

        GridNode best = null;
        float bestDist = float.MaxValue;

        for (int dx = -radius - 1; dx <= radius + 1; dx++)
        {
            for (int dy = -radius - 1; dy <= radius + 1; dy++)
            {
                int x = centerNode.GridX + dx;
                int y = centerNode.GridY + dy;

                GridNode node = _pathfinder.GridManager.GetNode(x, y);
                if (node != null && node.Walkable && !node.IsOccupied)
                {
                    float dist = Vector3.Distance(center, node.WorldPosition);
                    if (dist < bestDist)
                    {
                        best = node;
                        bestDist = dist;
                    }
                }
            }
        }

        if (best == null)
        {
            Debug.LogWarning($"[Targeting] No walkable + unoccupied node found near {center} with radius {radius}");
        }

        return best;
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


    BuildingHealth FindBestBuildingTarget()
    {


        var candidates = GameObject.FindObjectsOfType<BuildingHealth>();
        BuildingHealth best = null;
        float bestScore = float.MinValue;

        foreach (var bh in candidates)
        {
            var bhScript = bh.GetComponent<BuildingHealth>();
            if (bhScript == null) continue;

            float distance = Vector3.Distance(transform.position, bh.transform.position);
            float distanceScore = -distance;

            float priorityScore = GetPreferenceScore(bhScript.purpose);
            float totalScore = -priorityScore * 10f + distanceScore;

            if (totalScore > bestScore)
            {
                bestScore = totalScore;
                best = bh;
            }
        }

        if (best != null)
            Debug.Log($"[{name}] Best building found: {best.name}");
        else
            Debug.Log($"[{name}] No building found!");

        return best;
    }

    UnitInstance FindNearestEnemyInRange()
    {
        UnitInstance[] allUnits = GameObject.FindObjectsOfType<UnitInstance>();
        UnitInstance closest = null;
        float closestDist = _detectionRange;

        foreach (var other in allUnits)
        {
            if (other == this) continue;
            if (Vector3.Distance(transform.position, other.transform.position) < closestDist)
            {
                closestDist = Vector3.Distance(transform.position, other.transform.position);
                closest = other;
            }
        }

        return closest;
    }

    int GetPreferenceScore(BuildingPurpose purpose)
    {
        List<BuildingPurpose> prefs = _unitType.TargetPreference;
        return prefs.IndexOf(purpose) >= 0 ? prefs.IndexOf(purpose) : prefs.Count;
    }

    public void ForceSetCurrentNode(GridNode node)
    {
        _currentNode = node;
    }
}
