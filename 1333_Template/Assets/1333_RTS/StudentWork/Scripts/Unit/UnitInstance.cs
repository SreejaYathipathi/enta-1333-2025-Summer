using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ControlMode { Manual, AI }

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

    public int ArmyID { get; private set; }

    public UnitInstance GetTargetUnit() => targetUnit;
    public BuildingHealth GetTargetBuilding() => targetBuilding;

    public bool HasTargetUnit() => targetUnit != null;
    public bool HasTargetBuilding() => targetBuilding != null;

    public void ClearTargetUnit() => targetUnit = null;
    public void ClearTargetBuilding() => targetBuilding = null;

    public UnitType UnitType => _unitType;

    public int Damage => _unitType.damage;
    public int Range => _unitType.range;

    public bool IsMoving => _isMoving;

    private GridNode _previousNode;

    public List<GridNode> CurrentPath => _currentPath;

    private bool _atDestination = false;

    private UnitInstance dynamicChaseTarget = null;
    private float enemyDetectionRadius = 30f;

    public ControlMode Mode { get; private set; } = ControlMode.AI;

    private void Start()
    {
        string scene = SceneManager.GetActiveScene().name;

        if (ArmyID == 0)
        {
            // Player's army
            if (scene == "PlayerScene")
                SetControlMode(ControlMode.Manual);
            else
                SetControlMode(ControlMode.AI);
        }
        else if (ArmyID == 1)
        {
            // Enemy army is always AI
            SetControlMode(ControlMode.AI);

            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.material.color = Color.magenta;
            }
        }
    }

    public void SetControlMode(ControlMode mode)
    {
        Mode = mode;
    }

    public void Initialize(AStarPathFinding pathfinder, UnitType unitType)
    {
        _pathfinder = pathfinder;
        _unitType = unitType;
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState == GameState.GameOver)
            return;

        if (!_isMoving || _currentPath == null || _currentPath.Count == 0 || _pathIndex >= _currentPath.Count)
        {
            if (Mode == ControlMode.AI && targetBuilding == null)
            {
                EvaluateTarget();
            }

            return;
        }

        // Movement update
        if (_isMoving && _currentPath != null && _currentPath.Count > 0 && _pathIndex < _currentPath.Count)
        {
            GridNode nextNode = _currentPath[_pathIndex];
            if (!nextNode.Walkable)
            {
                Debug.LogWarning($"[Repath] {name} detected blocked node at {_pathIndex} ({nextNode.GridX},{nextNode.GridY}). Repathing...");
                if (_targetWorldPosition.HasValue)
                {
                    GridNode retryNode = _pathfinder.GridManager.GetNodeFromWorldPosition(_targetWorldPosition.Value);
                    if (retryNode != null && retryNode.Walkable)
                        TargetSet(_targetWorldPosition.Value);
                }
                return;
            }

            Vector3 nextWaypoint = new Vector3(nextNode.WorldPosition.x, transform.position.y, nextNode.WorldPosition.z);
            float step = _moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, nextWaypoint, step);

            GridNode nodeNow = _pathfinder.GridManager.GetNodeFromWorldPosition(transform.position);
            if (_currentNode != null && _currentNode != nodeNow && _currentNode.IsOccupied)
                _currentNode.IsOccupied = false;

            if (nodeNow != null && nodeNow != _currentNode)
                _currentNode = nodeNow;

            if (Vector3.Distance(transform.position, nextWaypoint) < 0.05f)
            {
                _pathIndex++;
                if (_pathIndex >= _currentPath.Count)
                {
                    _isMoving = false;
                    _atDestination = true;
                    if (_currentNode != null) _currentNode.IsOccupied = true;
                    _currentPath.Clear();
                }
            }
            return;
        }

        // --- AI Targeting ---
        if (Mode == ControlMode.AI)
        {
            // If we have a dynamic target, follow its current position
            if (dynamicChaseTarget != null)
            {
                TargetSet(dynamicChaseTarget.transform.position);
                // Stop chasing if target dies
                if (dynamicChaseTarget.Equals(null))
                    dynamicChaseTarget = null;
            }
            else if (targetUnit == null && targetBuilding == null)
            {
                EvaluateTarget();
            }
        }
    }

    public bool HasReachedDestination()
    {
        return _atDestination;
    }

    public void TargetSet(Vector3 worldPosition)
    {

        _atDestination = false;

        Debug.Log($"[SetTarget] {name} trying to move to {worldPosition}");
        Debug.Log($"[TargetSet] {name} moving to {worldPosition} | Called from: {new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name}");

        if (_pathfinder == null)
        {
            Debug.LogError($"[SetTarget] Pathfinder is NULL for {name}");
            return;
        }

        GridNode startNode = _pathfinder.GridManager.GetNodeFromWorldPosition(transform.position);

        if (_currentNode != null && _currentNode.IsOccupied)
            _currentNode.IsOccupied = false;

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
            _atDestination = true;
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

    public void EvaluateTarget()
    {
        if (GameManager.Instance.CurrentState == GameState.GameOver)
            return;

        Debug.Log($"[{name}] Evaluating target...");

        UnitInstance closestUnit = FindNearestEnemyInRange(enemyDetectionRadius);
        BuildingHealth bestBuilding = FindBestBuildingTarget();

        float unitDist = closestUnit ? Vector3.Distance(transform.position, closestUnit.transform.position) : Mathf.Infinity;
        float buildingDist = bestBuilding ? Vector3.Distance(transform.position, bestBuilding.transform.position) : Mathf.Infinity;

        if (unitDist < buildingDist)
        {
            targetUnit = closestUnit;
            targetBuilding = null;
            dynamicChaseTarget = closestUnit;
            Debug.Log($"[{name}] Targeting player unit: {closestUnit.name}");

            GridNode nearNode = GetNearbyValidNode(closestUnit.transform.position, new Vector2Int(1, 1));
            if (nearNode != null)
            {
                _targetWorldPosition = nearNode.WorldPosition;
                TargetSet(_targetWorldPosition.Value);
            }
            else
            {
                Debug.LogWarning($"[{name}] No valid adjacent node near {closestUnit.name}");
            }
        }
        else if (bestBuilding != null)
        {
            targetBuilding = bestBuilding;
            targetUnit = null;
            dynamicChaseTarget = null;
            Debug.Log($"[{name}] Targeting building: {bestBuilding.name}");

            GridNode nearNode = GetNearbyValidNode(bestBuilding.transform.position, bestBuilding.FootprintSize);
            if (nearNode != null)
            {
                _targetWorldPosition = nearNode.WorldPosition;
                TargetSet(_targetWorldPosition.Value);
            }
            else
            {
                Debug.LogWarning($"[{name}] No valid adjacent node near {bestBuilding.name}");
            }
        }
        else
        {
            targetUnit = null;
            targetBuilding = null;
            Debug.Log($"[{name}] No valid target found.");
        }
    }

    public void SetArmy(int armyId)
    {
        ArmyID = armyId;
    }

    private GridNode GetNearbyValidNode(Vector3 targetPosition, Vector2Int footprint)
    {
        GridNode best = null;
        float bestDist = float.MaxValue;

        best = SearchAdjacentNodes(targetPosition, footprint, 1);

        if (best == null)
        {
            Debug.LogWarning($"[Targeting] No adjacent free node near {targetPosition}, expanding search.");
            best = SearchAdjacentNodes(targetPosition, footprint, 2);
        }

        return best;
    }

    private GridNode SearchAdjacentNodes(Vector3 targetPosition, Vector2Int footprint, int margin)
    {
        GridNode best = null;
        float bestDist = float.MaxValue;

        Vector3 bottomLeft = targetPosition - new Vector3((footprint.x - 1) / 2f, 0, (footprint.y - 1) / 2f);

        for (int dx = -margin; dx < footprint.x + margin; dx++)
        {
            for (int dy = -margin; dy < footprint.y + margin; dy++)
            {
                Vector3 pos = bottomLeft + new Vector3(dx, 0, dy);
                GridNode node = _pathfinder.GridManager.GetNodeFromWorldPosition(pos);
                if (node == null || !node.Walkable || node.IsOccupied)
                    continue;

                float dist = Vector3.Distance(transform.position, node.WorldPosition);
                if (dist < bestDist)
                {
                    best = node;
                    bestDist = dist;
                }
            }
        }
        return best;
    }

    public void ForceStopMoving()
    {
        _isMoving = false;
        _currentPath.Clear();
        _pathIndex = 0;
        _atDestination = true;
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
            if (bh == null || bh.gameObject == null) continue;
            if (!bh.gameObject.activeInHierarchy) continue;

            // Skip buildings that belong to the same army
            if (bh.ArmyID == this.ArmyID)
                continue;

            float distance = Vector3.Distance(transform.position, bh.transform.position);
            float totalScore = 0f;

            if (SceneManager.GetActiveScene().name == "PlayerScene")
            {
                // In PlayerScene, just prefer closer buildings
                totalScore = -distance;
            }
            else
            {
                // In EnemyScene, use preference based on building purpose
                float priorityScore = GetPreferenceScore(bh.purpose);
                totalScore = -priorityScore * 10f + distance;
            }

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

    UnitInstance FindNearestEnemyInRange(float radius)
    {
        UnitInstance[] allUnits = GameObject.FindObjectsOfType<UnitInstance>();
        UnitInstance closest = null;
        float closestDist = radius;  // only within this radius

        foreach (var other in allUnits)
        {
            if (other == this) continue;
            if (other.ArmyID == this.ArmyID) continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
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

    public void MoveToAndCut(ObstacleCuttable obstacle)
    {
        // If obstacle was assigned to someone else, stop
        if (!obstacle.TryAssign(this))
        {
            Debug.Log($"[{name}] Obstacle already assigned to another unit");
            return;
        }

        GridNode nearNode = GetNearbyValidNode(obstacle.transform.position, new Vector2Int(1, 1));
        if (nearNode == null)
        {
            Debug.LogWarning($"[{name}] No valid node found near obstacle: {obstacle.name}");
            obstacle.Unassign(this); // free it if path failed
            return;
        }

        StartCoroutine(MoveAndCut(obstacle, nearNode.WorldPosition));
    }

    private IEnumerator MoveAndCut(ObstacleCuttable obstacle, Vector3 destination)
    {
        TargetSet(destination);

        while (_isMoving || Vector3.Distance(transform.position, destination) > 1f)
            yield return null;

        while (obstacle != null)
        {
            obstacle.Cut();
            yield return new WaitForSeconds(0.4f);
        }
    }
}
