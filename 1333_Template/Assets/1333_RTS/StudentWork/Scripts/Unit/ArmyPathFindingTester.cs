using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArmyPathFindingTester : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private AStarPathFinding _sharedPathfinder;
    [SerializeField] private List<ArmyComposition> armyCompositions = new();
    [SerializeField] private int patrolRange = 8;
    [SerializeField] private float detectionRange = 4f;

    private readonly List<ArmyManager> _armies = new();
    private enum UnitState { Patrol, Follow }
    private readonly Dictionary<UnitInstance, UnitState> _unitStates = new();
    private readonly Dictionary<UnitInstance, Vector3[]> _patrolPoints = new();
    private readonly Dictionary<UnitInstance, int> _patrolTargetIndex = new();
    private readonly Dictionary<UnitInstance, UnitInstance> _followTargets = new();
    private readonly Dictionary<UnitInstance, Vector3> _lastKnownEnemyPos = new();

    private static readonly Color[] ArmyColors = new Color[]
    {
            Color.cyan, Color.red, Color.yellow, Color.green, Color.magenta, Color.blue, Color.white, Color.black
    };

    private void Start()
    {
        _sharedPathfinder = new AStarPathFinding(gridManager);
        _armies.Clear();

        for (int i = 0; i < armyCompositions.Count; i++)
        {
            ArmyManager army = new ArmyManager { ArmyID = i + 1, GridManager = gridManager };
            SpawnArmyUnits(army, armyCompositions[i]);
            _armies.Add(army);
        }
    }

    private void SpawnArmyUnits(ArmyManager army, ArmyComposition composition)
    {
        foreach (var entry in composition.units)
        {
            for (int i = 0; i < entry.count; i++)
            {
                int attempts = 0;
                int maxAttempts = 1000;
                Vector3 spawnPos = Vector3.zero;
                bool found = false;
                int unitWidth = entry.unitTypePrefab.unitType.Width;
                int unitHeight = entry.unitTypePrefab.unitType.Height;
                while (!found && attempts < maxAttempts)
                {
                    int x = Random.Range(0, gridManager.GridSettings.GridSizeX - unitWidth + 1);
                    int y = Random.Range(0, gridManager.GridSettings.GridSizeY - unitHeight + 1);
                    if (IsRegionWalkable(x, y, unitWidth, unitHeight))
                    {
                        spawnPos = gridManager.GetNode(x, y).WorldPosition;
                        found = true;
                    }
                    attempts++;
                }
                if (!found)
                {
                    Debug.LogWarning($"Failed to find valid spawn position for unit {entry.unitTypePrefab.unitType.name}.");
                    continue;
                }
                GameObject go = Instantiate(entry.unitTypePrefab.prefab, spawnPos, Quaternion.identity);
                UnitInstance unit = go.GetComponent<UnitInstance>();
                unit.Initialize(_sharedPathfinder, entry.unitTypePrefab.unitType);
                army.Units.Add(unit);
                _unitStates[unit] = UnitState.Patrol;
                _patrolPoints[unit] = new Vector3[2] {
                        GetRandomPatrolPoint(spawnPos, unit.Width, unit.Height),
                        GetRandomPatrolPoint(spawnPos, unit.Width, unit.Height)
                    };
                _patrolTargetIndex[unit] = 0;
            }
        }
    }

    private bool IsRegionWalkable(int x, int y, int width, int height)
    {
        for (int dx = 0; dx < width; dx++)
        {
            for (int dy = 0; dy < height; dy++)
            {
                if (!gridManager.GetNode(x + dx, y + dy).Walkable)
                    return false;
            }
        }
        return true;
    }

    private Vector3 GetRandomPatrolPoint(Vector3 origin, int unitWidth, int unitHeight)
    {
        GridNode node = gridManager.GetNodeFromWorldPosition(origin);
        float nodeSize = gridManager.GridSettings.NodeSize;
        int nodeX = Mathf.RoundToInt(node.WorldPosition.x / nodeSize);
        int nodeY = Mathf.RoundToInt(node.WorldPosition.z / nodeSize);
        int x = Mathf.Clamp(Random.Range(nodeX - patrolRange, nodeX + patrolRange), 0, gridManager.GridSettings.GridSizeX - 1);
        int y = Mathf.Clamp(Random.Range(nodeY - patrolRange, nodeY + patrolRange), 0, gridManager.GridSettings.GridSizeY - 1);
        for (int tries = 0; tries < 20; tries++)
        {
            int tryX = Mathf.Clamp(x + Random.Range(-patrolRange, patrolRange), 0, gridManager.GridSettings.GridSizeX - unitWidth);
            int tryY = Mathf.Clamp(y + Random.Range(-patrolRange, patrolRange), 0, gridManager.GridSettings.GridSizeY - unitHeight);
            if (IsRegionWalkable(tryX, tryY, unitWidth, unitHeight))
                return gridManager.GetNode(tryX, tryY).WorldPosition;
        }
        return node.WorldPosition;
    }

    private void Update()
    {
        for (int i = 0; i < _armies.Count; i++)
        {
            ArmyManager ownArmy = _armies[i];
            List<UnitInstance> enemyUnits = new();
            for (int j = 0; j < _armies.Count; j++)
            {
                if (i == j) continue;
                enemyUnits.AddRange(_armies[j].Units.Select(x => x as UnitInstance));
            }
            UpdateArmyUnits(ownArmy, enemyUnits);
        }
    }

    private void UpdateArmyUnits(ArmyManager ownArmy, List<UnitInstance> enemyUnits)
    {
        foreach (UnitInstance unit in ownArmy.Units)
        {
            if (unit == null) continue;
            UnitState state = _unitStates[unit];
            switch (state)
            {
                case UnitState.Patrol:
                    UnitInstance enemy = FindNearestEnemy(unit, enemyUnits);
                    if (enemy != null)
                    {
                        _unitStates[unit] = UnitState.Follow;
                        _followTargets[unit] = enemy;
                        _lastKnownEnemyPos[unit] = enemy.transform.position;
                        unit.SetTarget(enemy.transform.position);
                    }
                    else
                    {
                        PatrolBehavior(unit);
                    }
                    break;
                case UnitState.Follow:
                    if (!_followTargets.ContainsKey(unit) || _followTargets[unit] == null)
                    {
                        _unitStates[unit] = UnitState.Patrol;
                        break;
                    }
                    UnitInstance target = _followTargets[unit];
                    if (Vector3.Distance(_lastKnownEnemyPos[unit], target.transform.position) > 0.5f)
                    {
                        _lastKnownEnemyPos[unit] = target.transform.position;
                        unit.SetTarget(target.transform.position);
                    }
                    if (Vector3.Distance(unit.transform.position, target.transform.position) > detectionRange * 2)
                    {
                        _unitStates[unit] = UnitState.Patrol;
                        break;
                    }
                    break;
            }
        }
    }

    private void PatrolBehavior(UnitInstance unit)
    {
        Vector3[] points = _patrolPoints[unit];
        int idx = _patrolTargetIndex[unit];
        if (Vector3.Distance(unit.transform.position, points[idx]) < 0.2f)
        {
            idx = 1 - idx;
            _patrolTargetIndex[unit] = idx;
            points[idx] = GetRandomPatrolPoint(unit.transform.position, unit.Width, unit.Height);
            unit.SetTarget(points[idx]);
        }
        else if (!unit.IsMoving)
        {
            unit.SetTarget(points[idx]);
        }
    }

    private UnitInstance FindNearestEnemy(UnitInstance unit, List<UnitInstance> enemyUnits)
    {
        float minDist = detectionRange;
        UnitInstance nearest = null;
        foreach (UnitInstance enemy in enemyUnits)
        {
            if (enemy == null) continue;
            float dist = Vector3.Distance(unit.transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemy;
            }
        }
        return nearest;
    }

    private void OnDrawGizmos()
    {
        for (int armyIdx = 0; armyIdx < _armies.Count; armyIdx++)
        {
            ArmyManager army = _armies[armyIdx];
            Color color = ArmyColors[armyIdx % ArmyColors.Length];
            foreach (UnitInstance unit in army.Units)
            {
                if (unit == null || unit.CurrentPath == null || unit.CurrentPath.Count < 2)
                    continue;
                Gizmos.color = color;
                for (int i = 0; i < unit.CurrentPath.Count - 1; i++)
                {
                    Gizmos.DrawLine(unit.CurrentPath[i].WorldPosition, unit.CurrentPath[i + 1].WorldPosition);
                }
            }
        }
    }
}
