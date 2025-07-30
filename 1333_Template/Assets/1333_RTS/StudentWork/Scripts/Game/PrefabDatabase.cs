using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Prefab Database")]
public class PrefabDatabase : ScriptableObject
{
    public List<GameObject> buildingPrefabs;
    public List<GameObject> obstaclePrefabs;

    public GameObject GetPrefabByName(string name)
    {
        // Search buildings
        foreach (var prefab in buildingPrefabs)
        {
            if (prefab != null && prefab.name == name)
                return prefab;
        }

        // Search obstacles
        foreach (var prefab in obstaclePrefabs)
        {
            if (prefab != null && prefab.name == name)
                return prefab;
        }

        Debug.LogError($"[PrefabDatabase] Prefab not found: {name}");
        return null;
    }
}
