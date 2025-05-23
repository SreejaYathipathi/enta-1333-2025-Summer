using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private PathFinder pathfinder;

    private void Awake()
    {
        gridManager.InitializeGrid();
        //unitManager.SpawnDummyUnit();
    }

    private void Start()
    {
        Vector2Int start = new Vector2Int(0, 0);
        Vector2Int goal = new Vector2Int(9, 9);
        pathfinder.FindPath(start, goal);
    }
}
