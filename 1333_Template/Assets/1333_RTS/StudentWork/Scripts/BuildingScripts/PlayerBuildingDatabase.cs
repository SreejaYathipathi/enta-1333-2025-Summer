using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Player Building Database")]
public class PlayerBuildingDatabase : ScriptableObject
{
    public List<GameObject> buildingPrefabs;

    public GameObject GetPrefabByName(string name)
    {
        foreach (var prefab in buildingPrefabs)
        {
            if (prefab.name == name)
                return prefab;
        }
        return null;
    }
}