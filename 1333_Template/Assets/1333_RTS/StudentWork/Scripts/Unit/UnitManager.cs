using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    // Reference to the GridManager that manages the grid system
    [SerializeField] private GridManager _gridManager;

    // Dictionary that maps army IDs to their respective ArmyManager instances
    private Dictionary<int, ArmyManager> _armyManager;

    // Shortcut to access the player army (assumed to be at index 0)
    public ArmyManager PlayerArmy => _armyManager?[0];

    // Spawns a dummy unit at a random location on the grid (currently logs position only)
    public void SpawnDummyUnit(Transform parent)
    {
        // Ensure the grid is initialized before attempting to spawn
        if (!_gridManager.IsInitialized)
        {
            //Debug.LogError("Grid not initialized!");
            return;
        }

        // Generate random X and Y coordinates within grid bounds
        int randomX = Random.Range(0, _gridManager.GridSettings.GridSizeX);
        int randomY = Random.Range(0, _gridManager.GridSettings.GridSizeY);

        // Get the node at the random grid location
        GridNode spawnNode = _gridManager.GetNode(randomX, randomY);

        // Log the spawn coordinates and their corresponding world position
        //Debug.Log($"Dummy unit spawned at ({randomX}, {randomY}) - World Position: {spawnNode.WorldPosition}");
    }
}
