using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GridManager : MonoBehaviour
{
    [SerializeField] private GridSettings gridSettings;
    [SerializeField] private List<TerrainType> terrainTypes;
    public GridSettings GridSettings => gridSettings;

    public List<GridNode> Path = new List<GridNode>();
    public HashSet<GridNode> Visited = new HashSet<GridNode>();
    public List<GridNode> Frontier = new List<GridNode>();

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

    public GridNode GetNode(int x, int y)
    {
        if (x >= 0 && x < gridSettings.GridSizeX && y >= 0 && y < gridSettings.GridSizeY)
            return gridNodes[x, y];
        return null;
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

                if (Path.Contains(node))
                {
                    Gizmos.color = Color.blue;
                }
                else if (Frontier.Contains(node))
                {
                    Gizmos.color = Color.yellow;
                }
                else if (Visited.Contains(node))
                {
                    Gizmos.color = Color.red;
                }
                else
                {
                    Gizmos.color = node.TerrainType.GizmoColor;
                }

                Gizmos.DrawWireCube(node.WorldPosition, Vector3.one * gridSettings.NodeSize * 0.9f);
            }
        }
    }

    public List<GridNode> GetNeighbours(GridNode node)
    {
        List<GridNode> neighbours = new List<GridNode>();
        Vector3 pos = node.WorldPosition;

        int x = Mathf.RoundToInt(pos.x / gridSettings.NodeSize);
        int y = Mathf.RoundToInt(pos.z / gridSettings.NodeSize);

        int[,] directions = new int[,] { { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 } };

        for (int i = 0; i < directions.GetLength(0); i++)
        {
            GridNode neighbour = GetNode(x + directions[i, 0], y + directions[i, 1]);
            if (neighbour != null && neighbour.Walkable)
            {
                neighbours.Add(neighbour);
            }
        }
        return neighbours;
    }

    public GridNode GetNodeFromWorldPosition(Vector3 worldPos)
    {
        /*int x = Mathf.RoundToInt(worldPos.x / gridSettings.NodeSize);
        int y = Mathf.RoundToInt(worldPos.z / gridSettings.NodeSize);
        return GetNode(x, y);*/

        int x = gridSettings.UseXZPlane ? Mathf.RoundToInt(worldPos.x / gridSettings.NodeSize) : Mathf.RoundToInt(worldPos.x / gridSettings.NodeSize);
        int y = gridSettings.UseXZPlane ? Mathf.RoundToInt(worldPos.z / gridSettings.NodeSize) : Mathf.RoundToInt(worldPos.y / gridSettings.NodeSize);
        x = Mathf.Clamp(x, 0, gridSettings.GridSizeX - 1);
        y = Mathf.Clamp(y, 0, gridSettings.GridSizeY - 1);
        return GetNode(x, y);
    }
}
