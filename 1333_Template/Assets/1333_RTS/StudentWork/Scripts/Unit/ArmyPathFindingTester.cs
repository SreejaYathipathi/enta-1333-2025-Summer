using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Spawns player-controlled units, assigns them to an ArmyManager,
/// and lets them patrol or accept command inputs.  
/// </summary>
public class ArmyPathFindingTester : MonoBehaviour
{
    // Serialized references / config
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private AStarPathFinding _sharedPathfinder;
    [SerializeField] private ArmyComposition _playerArmyComposition;
    [SerializeField] private int _patrolRange = 8;

    public AStarPathFinding SharedPathfinder => _sharedPathfinder;

    // Runtime containers
    private readonly List<ArmyManager> _armies = new();
    private readonly Dictionary<UnitInstance, UnitState> _unitStates = new();
    private readonly Dictionary<UnitInstance, Vector3[]> _patrolPoints = new();
    private readonly Dictionary<UnitInstance, int> _patrolTargetIndex = new();

    private enum UnitState { Patrol, Command }

    public ArmyManager PlayerArmy => _armies.Count > 0 ? _armies[0] : null;

    private void Awake()
    {
        _sharedPathfinder = new AStarPathFinding(_gridManager);
    }

    private void Start()
    {
        _armies.Clear();

        ArmyManager playerArmy = new ArmyManager { ArmyID = 0, GridManager = _gridManager };
        _armies.Add(playerArmy);
    }


    /// <summary>
    /// Instantiates units described by an ArmyComposition asset.
    /// </summary>
    public void SpawnPlayerUnits(ArmyComposition composition)
    {
        ArmyManager playerArmy = _armies[0];

        foreach (var entry in composition.units)
        {
            for (int i = 0; i < entry.count; i++)
            {
                Vector3 pos = GetRandomValidPosition(entry.unitTypePrefab.unitType.Width, entry.unitTypePrefab.unitType.Height);
                if (pos == Vector3.zero) continue;

                Vector3 lifted = pos + Vector3.up * (_gridManager.GridSettings.NodeSize / 2.5f + 0.1f);
                GameObject go = Instantiate(entry.unitTypePrefab.prefab, lifted, Quaternion.identity);
                UnitInstance unit = go.GetComponent<UnitInstance>();
                unit.Initialize(_sharedPathfinder, entry.unitTypePrefab.unitType);
                unit.SetArmy(0);
                playerArmy.Units.Add(unit);

                // Set state: Patrol in PlayerScene, Command in EnemyScene
                string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                ////Debug.Log("[Spawn] Current Scene: " + scene);
                if (scene == "PlayerScene")
                {
                    _unitStates[unit] = UnitState.Patrol;
                    _patrolPoints[unit] = new Vector3[2]
                    {
                        GetRandomPatrolPoint(pos),
                        GetRandomPatrolPoint(pos)
                    };
                    _patrolTargetIndex[unit] = 0;
                    StartCoroutine(PatrolLoop(unit));
                }
                else
                {
                    _unitStates[unit] = UnitState.Command;
                }
            }
        }
    }

    /// <summary>
    /// Called by external scripts when a freshly spawned unit should start patrolling.
    /// </summary>
    public void RegisterPatrollingUnit(UnitInstance unit)
    {
        StartCoroutine(RegisterPatrolNextFrame(unit));
    }

    private IEnumerator RegisterPatrolNextFrame(UnitInstance unit)
    {
        yield return null; // Wait one frame to ensure node is assigned

        _unitStates[unit] = UnitState.Patrol;
        _patrolPoints[unit] = new Vector3[2]
        {
        GetRandomPatrolPoint(unit.transform.position),
        GetRandomPatrolPoint(unit.transform.position)
        };
        _patrolTargetIndex[unit] = 0;
        StartCoroutine(PatrolLoop(unit));
        //Debug.Log($"[PatrolRegister] {unit.name} registered for patrol.");
    }

    // Patrol behaviour coroutine
    private IEnumerator PatrolLoop(UnitInstance unit)
    {
        while (unit != null && _unitStates.ContainsKey(unit) && _unitStates[unit] == UnitState.Patrol)
        {

            if (GameManager.Instance.CurrentState == GameState.GameOver)
                yield break;

            // Stop patrolling if wave is active or unit is in AI mode
            if (EnemySpawner.WaveActive || unit.Mode != ControlMode.Manual)
            {
                yield return null; // wait and recheck
                continue;
            }

            Vector3[] points = _patrolPoints[unit];
            int idx = _patrolTargetIndex[unit];

            // Only assign target if far enough
            if (Vector3.Distance(unit.transform.position, points[idx]) > 0.2f)
            {
                unit.TargetSet(points[idx]);
            }

            // Wait until it reaches the current patrol point
            while (!unit.HasReachedDestination())
                yield return null;

            // Add a wait between moves (customize as needed)
            yield return new WaitForSeconds(Random.Range(2f, 4f));

            // Flip to next point and refresh it
            idx = 1 - idx;
            _patrolTargetIndex[unit] = idx;
            points[idx] = GetRandomPatrolPoint(unit.transform.position);
        }
    }

    // Finds a random walkable world-space position big enough for a unit’s footprint.
    private Vector3 GetRandomValidPosition(int width, int height)
    {
        for (int i = 0; i < 100; i++)
        {
            int x = Random.Range(0, _gridManager.GridSettings.GridSizeX - width);
            int y = Random.Range(0, _gridManager.GridSettings.GridSizeY - height);
            if (IsRegionWalkable(x, y, width, height))
                return _gridManager.GetNode(x, y).WorldPosition;
        }
        return Vector3.zero;
    }

    // Picks a random patrol waypoint within _patrolRange of the origin node.
    private Vector3 GetRandomPatrolPoint(Vector3 origin)
    {
        GridNode node = _gridManager.GetNodeFromWorldPosition(origin);
        int x = Random.Range(node.GridX - _patrolRange, node.GridX + _patrolRange);
        int y = Random.Range(node.GridY - _patrolRange, node.GridY + _patrolRange);
        x = Mathf.Clamp(x, 0, _gridManager.GridSettings.GridSizeX - 1);
        y = Mathf.Clamp(y, 0, _gridManager.GridSettings.GridSizeY - 1);
        return _gridManager.GetNode(x, y).WorldPosition;
    }

    // Checks if every node in a width×height rectangle is walkable.
    private bool IsRegionWalkable(int x, int y, int width, int height)
    {
        for (int dx = 0; dx < width; dx++)
        {
            for (int dy = 0; dy < height; dy++)
            {
                if (!_gridManager.GetNode(x + dx, y + dy).Walkable)
                    return false;
            }
        }
        return true;
    }
}
