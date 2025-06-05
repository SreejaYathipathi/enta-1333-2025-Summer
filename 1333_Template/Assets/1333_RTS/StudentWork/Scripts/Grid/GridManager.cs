using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GridManager : MonoBehaviour
{
    [SerializeField] private GridSettings _gridSettings;
    [SerializeField] private List<TerrainType> _terrainTypes;
    public GridSettings GridSettings => _gridSettings;

    public List<GridNode> Path = new List<GridNode>();
    public HashSet<GridNode> Visited = new HashSet<GridNode>();
    public List<GridNode> Frontier = new List<GridNode>();

    private GridNode[,] _gridNodes;

    public bool IsInitialized { get; private set; } = false;

    public void InitializeGrid()
    {
        _gridNodes = new GridNode[_gridSettings.GridSizeX, _gridSettings.GridSizeY];
        for (int x = 0; x < _gridSettings.GridSizeX; x++)
        {
            for (int y = 0; y < _gridSettings.GridSizeY; y++)
            {
                Vector3 worldPos = _gridSettings.UseXZPlane
                    ? new Vector3 (x, 0, y) * _gridSettings.NodeSize
                    : new Vector3 (x, y, 0) * _gridSettings.NodeSize;

                TerrainType randomTerrain = _terrainTypes[Random.Range(0, _terrainTypes.Count)];

                GridNode node = new GridNode
                {
                    Name = $"Cell_{(x + _gridSettings.GridSizeX * x + y)}",
                    WorldPosition = worldPos,
                    Walkable = randomTerrain.IsWalkable,
                    Weight = randomTerrain.MovementCost,
                    TerrainType = randomTerrain
                };
                _gridNodes[x, y] = node;
            }
        }
        IsInitialized = true;
    }

    public GridNode GetNode(int x, int y)
    {
        if (x >= 0 && x < _gridSettings.GridSizeX && y >= 0 && y < _gridSettings.GridSizeY)
            return _gridNodes[x, y];
        return null;
    }

    public void SetWalkable(int x, int y, bool walkable)
    {
        GridNode node = _gridNodes[x, y];
        node.Walkable = walkable;
        _gridNodes[x, y] = node;
    }

    private void OnDrawGizmos()
    {
        if (_gridNodes == null || _gridSettings == null) return;

        for (int x = 0; x < _gridSettings.GridSizeX; x++)
        {
            for (int y = 0; y < _gridSettings.GridSizeY; y++)
            {
                GridNode node = _gridNodes[x, y];

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

                Gizmos.DrawWireCube(node.WorldPosition, Vector3.one * _gridSettings.NodeSize * 0.9f);
            }
        }
    }

    public List<GridNode> GetNeighbours(GridNode node)
    {
        List<GridNode> neighbours = new List<GridNode>();
        Vector3 pos = node.WorldPosition;

        int x = Mathf.RoundToInt(pos.x / _gridSettings.NodeSize);
        int y = Mathf.RoundToInt(pos.z / _gridSettings.NodeSize);

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

        int x = _gridSettings.UseXZPlane ? Mathf.RoundToInt(worldPos.x / _gridSettings.NodeSize) : Mathf.RoundToInt(worldPos.x / _gridSettings.NodeSize);
        int y = _gridSettings.UseXZPlane ? Mathf.RoundToInt(worldPos.z / _gridSettings.NodeSize) : Mathf.RoundToInt(worldPos.y / _gridSettings.NodeSize);
        x = Mathf.Clamp(x, 0, _gridSettings.GridSizeX - 1);
        y = Mathf.Clamp(y, 0, _gridSettings.GridSizeY - 1);
        return GetNode(x, y);
    }
}
