/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelFromImage : MonoBehaviour
{
    public Texture2D mapImage;
    public float tileSize = 1f;
    public GridManager gridManager;

    [System.Serializable]
    public class ColorMapping
    {
        public Color color;
        public TerrainType terrain;
        public GameObject prefab;
    }

    public List<ColorMapping> colorMappings;

    private Dictionary<Color, ColorMapping> lookup;

    void Awake()
    {
        lookup = new();
        foreach (var entry in colorMappings)
            lookup[entry.color] = entry;

        int width = mapImage.width;
        int height = mapImage.height;

        TerrainType[,] terrainMap = new TerrainType[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color color = mapImage.GetPixel(x, y);
                if (lookup.TryGetValue(color, out var mapping))
                {
                    terrainMap[x, y] = mapping.terrain;

                    if (mapping.prefab != null)
                    {
                        Vector3 pos = new Vector3(x * tileSize, 0, y * tileSize);
                        Instantiate(mapping.prefab, pos, Quaternion.identity);
                    }
                }
            }
        }

        gridManager.InitializeGrid(terrainMap);
    }
}
*/