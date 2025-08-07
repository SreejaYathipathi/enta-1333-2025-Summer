using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Generates and manages the grid of GridNodes used for path-finding/building.
//[ExecuteAlways]
public class GridManager : MonoBehaviour
{
    [SerializeField] private GridSettings _gridSettings;
    [SerializeField] private List<TerrainType> _terrainTypes;

    [SerializeField] private Texture2D _mapImage;
    [SerializeField] private float _tileSize = 1f;

    [System.Serializable]
    public class ColorMapping
    {
        public Color color;
        public TerrainType terrain;
    }
    [SerializeField] private List<ColorMapping> _colorMappings;

    public GridSettings GridSettings => _gridSettings;

    private GridNode[,] _gridNodes;

    public bool IsInitialized { get; private set; } = false;


    // Builds the grid, instantiates terrain prefabs, and sets up node data.
    public void InitializeGrid()
    {
        _gridNodes = new GridNode[_gridSettings.GridSizeX, _gridSettings.GridSizeY];

        Dictionary<Color, ColorMapping> lookup = new();
        foreach (var entry in _colorMappings)
            lookup[entry.color] = entry;

        bool hasImage = _mapImage != null;

        for (int x = 0; x < _gridSettings.GridSizeX; x++)
        {
            for (int y = 0; y < _gridSettings.GridSizeY; y++)
            {
                Vector3 worldPos = new Vector3(x, 0, y) * _gridSettings.NodeSize;

                TerrainType terrain;
                GameObject prefab = null;

                if (hasImage)
                {
                    Color pixelColor = _mapImage.GetPixel(x, y);
                    if (lookup.TryGetValue(pixelColor, out var mapping))
                    {
                        terrain = mapping.terrain;
                        prefab = terrain.Prefab;
                    }
                    else
                    {
                        //Debug.LogWarning($"No mapping for color {pixelColor} at ({x},{y})");
                        continue;
                    }
                }
                else
                {
                    terrain = _terrainTypes[Random.Range(0, _terrainTypes.Count)];
                    prefab = terrain.Prefab;
                }

                _gridNodes[x, y] = new GridNode
                {
                    GridX = x,
                    GridY = y,
                    WorldPosition = worldPos,
                    TerrainType = terrain,
                    Walkable = terrain.IsWalkable,
                    Weight = terrain.MovementCost,
                    Name = $"Cell_{x}_{y}"
                };

                if (prefab != null)
                {
                    Instantiate(prefab, worldPos, Quaternion.identity);
                }
            }
        }

        IsInitialized = true;
    }

    // Returns the node at x,y or null if out of bounds.
    public GridNode GetNode(int x, int y)
    {
        if (x >= 0 && x < _gridSettings.GridSizeX && y >= 0 && y < _gridSettings.GridSizeY)
            return _gridNodes[x, y];
        return null;
    }

    // Sets the walkable flag on a node.
    public void SetWalkable(int x, int y, bool walkable)
    {
        GridNode node = _gridNodes[x, y];
        node.Walkable = walkable;
        _gridNodes[x, y] = node;
    }

    // Draws coloured gizmos in the editor to visualise grid cells
    private void OnDrawGizmos()
    {
        if (_gridNodes == null || _gridSettings == null) return;

        for (int x = 0; x < _gridSettings.GridSizeX; x++)
        {
            for (int y = 0; y < _gridSettings.GridSizeY; y++)
            {
                GridNode node = _gridNodes[x, y];

                /*if (Path.Contains(node))
                {
                    Gizmos.color = Color.blue;
                }
                else if (Frontier.Contains(node))
                {
                    Gizmos.color = Color.yellow;
                }
                else if (Visited.Contains(node))
                {
                    Gizmos.color = Color.magenta;
                }*/
                if (node.IsOccupied)
                {
                    Gizmos.color = new Color(1f, 0f, 0f, 0.8f);
                }
                else
                {
                    Gizmos.color = node.TerrainType.GizmoColor;
                }

                Gizmos.DrawWireCube(node.WorldPosition, Vector3.one * _gridSettings.NodeSize * 0.9f);
            }
        }
    }

    // Returns a flat list of every node (useful for A* resets).
    public List<GridNode> GetAllNodes()
    {
        List<GridNode> all = new List<GridNode>();
        for (int x = 0; x < GridSettings.GridSizeX; x++)
        {
            for (int y = 0; y < GridSettings.GridSizeY; y++)
            {
                all.Add(_gridNodes[x, y]);
            }
        }
        return all;
    }

    // Returns the four orthogonal neighbour nodes that are walkable.
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

    // Converts a world position to its containing node or null if outside grid.
    public GridNode GetNodeFromWorldPosition(Vector3 worldPos)
    {
        int x = _gridSettings.UseXZPlane ? Mathf.RoundToInt(worldPos.x / _gridSettings.NodeSize) : Mathf.RoundToInt(worldPos.x / _gridSettings.NodeSize);
        int y = _gridSettings.UseXZPlane ? Mathf.RoundToInt(worldPos.z / _gridSettings.NodeSize) : Mathf.RoundToInt(worldPos.y / _gridSettings.NodeSize);

        if (x < 0 || x >= _gridSettings.GridSizeX || y < 0 || y >= _gridSettings.GridSizeY)
            return null;

        return GetNode(x, y);

    }

    // Clamps a world position so it stays inside the grid bounds.
    public Vector3 ClampWorldToGrid(Vector3 worldPos)
    {
        float nodeSize = _gridSettings.NodeSize;
        int maxX = _gridSettings.GridSizeX - 1;
        int maxY = _gridSettings.GridSizeY - 1;

        float clampedX = Mathf.Clamp(worldPos.x, 0, maxX * nodeSize);
        float clampedZ = Mathf.Clamp(worldPos.z, 0, maxY * nodeSize);

        return new Vector3(clampedX, worldPos.y, clampedZ);
    }
}
