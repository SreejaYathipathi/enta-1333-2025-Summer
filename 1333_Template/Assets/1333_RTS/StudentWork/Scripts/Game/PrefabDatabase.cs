using System.Collections.Generic;
using UnityEngine;

// Stores references to building and obstacle prefabs so other systems can look them up by name.
[CreateAssetMenu(menuName = "Game/Prefab Database")]
public class PrefabDatabase : ScriptableObject
{
    // Change this to hold the item-data assets, not the raw prefabs
    public List<BuildingItemData> buildingItems;
    public List<GameObject> obstaclePrefabs;

    // Returns the BuildingItemData whose prefab matches the given name.
    public BuildingItemData GetItemDataByPrefabName(string prefabName)
    {
        foreach (var item in buildingItems)
            if (item != null && item.prefab != null && item.prefab.name == prefabName)
                return item;

        return null;
    }

    // Finds and returns any prefab (building or obstacle) that matches the name.
    public GameObject GetPrefabByName(string prefabName)
    {
        // first check building items
        var item = GetItemDataByPrefabName(prefabName);
        if (item != null) return item.prefab;

        // then obstacles
        foreach (var o in obstaclePrefabs)
            if (o != null && o.name == prefabName)
                return o;

        //Debug.LogError($"[PrefabDatabase] Prefab not found: {prefabName}");
        return null;
    }
}
