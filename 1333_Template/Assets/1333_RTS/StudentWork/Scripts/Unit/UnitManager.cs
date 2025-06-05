using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [SerializeField] private GridManager _gridManager;

    private Dictionary<int, ArmyManager> _armyManager;

    public ArmyManager PlayerArmy => _armyManager?[0];

    public void SpawnDummyUnit(Transform parent)
    {
        if (!_gridManager.IsInitialized)
        {
            Debug.LogError("Grid not initialized!");
            return;
        }

        int randomX = Random.Range(0, _gridManager.GridSettings.GridSizeX);
        int randomY = Random.Range(0, _gridManager.GridSettings.GridSizeY);

        GridNode spawnNode = _gridManager.GetNode(randomX, randomY);
        Debug.Log($"Dummy unit spawned at ({randomX}, {randomY}) - World Position: {spawnNode.WorldPosition}");

    }
}
