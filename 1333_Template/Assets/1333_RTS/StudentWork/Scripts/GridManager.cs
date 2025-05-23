using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private GridSettings gridSettings;
    [SerializeField] private List<TerrainType> terrainTypes;
    public GridSettings GridSettings => gridSettings;

    private GridNode[,] gridNodes;

    public bool IsInitialized { get; private set; } = false;

    public void InitializeGrid()
    {
        gridNodes = new GridNode[gridSettings.GridSizeX, gridSettings.GridSizeY];
        for (int x = 0; x < gridSettings.GridSizeX; x++)
        {
            for (int y = 0; y < gridSettings.GridSizeY; y++)
            {
                Vector3 worldPos = gridSettings.UseXZPlane
                    ? new Vector3 (x, 0, y) * gridSettings.NodeSize
                    : new Vector3 (x, y, 0) * gridSettings.NodeSize;

                TerrainType randomTerrain = terrainTypes[Random.Range(0, terrainTypes.Count)];

                GridNode node = new GridNode
                {
                    Name = $"Cell_{(x + gridSettings.GridSizeX * x + y)}",
                    WorldPosition = worldPos,
                    Walkable = randomTerrain.IsWalkable,
                    Weight = randomTerrain.MovementCost,
                    TerrainType = randomTerrain
                };
                gridNodes[x, y] = node;
            }
        }
        IsInitialized = true;
    }

    public GridNode? GetNode(int x, int y)
    {
        if (x < 0 || x >= gridSettings.GridSizeX || y < 0 || y >= gridSettings.GridSizeY)
        {
            return null;
        }
        return gridNodes[x, y];
    }

    public void SetWalkable(int x, int y, bool walkable)
    {
        GridNode node = gridNodes[x, y];
        node.Walkable = walkable;
        gridNodes[x, y] = node;
    }

    private void OnDrawGizmos()
    {
        if (gridNodes == null || gridSettings == null) return;

        for (int x = 0; x < gridSettings.GridSizeX; x++)
        {
            for (int y = 0; y < gridSettings.GridSizeY; y++)
            {
                GridNode node = gridNodes[x, y];
                Gizmos.color = node.TerrainType.GizmoColor;
                Gizmos.DrawWireCube(node.WorldPosition, Vector3.one * gridSettings.NodeSize * 0.9f);
            }
        }
    }
}
