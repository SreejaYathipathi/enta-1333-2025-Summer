using System.Collections.Generic;
using UnityEngine;

public class EnemyBaseLoader : MonoBehaviour
{
    [SerializeField] private EnemyBaseLayout _layout;
    [SerializeField] private GridManager _gridManager;

    private void Start()
    {
        foreach (var building in _layout.buildings)
        {
            Vector3 spawnPos = _gridManager.ClampWorldToGrid(building.position);

            GameObject placed = Instantiate(
                building.prefab,
                spawnPos,
                Quaternion.Euler(-90f, building.rotationY, 0f)
            );
            placed.name = $"[Enemy] {building.prefab.name}";

            Vector2Int size = Vector2Int.one;
            BuildingItemReference itemRef = building.prefab.GetComponent<BuildingItemReference>();
            if (itemRef != null)
                size = itemRef.Data.footprintSize;

            Vector3 basePos = spawnPos - GetFootprintOffset(size);
            for (int dx = 0; dx < size.x; dx++)
            {
                for (int dy = 0; dy < size.y; dy++)
                {
                    Vector3 pos = basePos + new Vector3(dx, 0, dy);
                    GridNode node = _gridManager.GetNodeFromWorldPosition(pos);
                    if (node != null)
                        node.IsOccupied = true;
                }
            }
        }


        Debug.Log("[EnemyBaseLoader] Enemy base loaded.");
    }

    private Vector3 GetFootprintOffset(Vector2Int size)
    {
        return new Vector3((size.x - 1) / 2f, 0f, (size.y - 1) / 2f);
    }
}
