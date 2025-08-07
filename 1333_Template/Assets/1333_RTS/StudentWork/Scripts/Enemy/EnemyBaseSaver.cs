#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// Saves and clears enemy base prefabs in the scene
public class EnemyBaseSaver
{
    [MenuItem("Tools/Save Enemy Base Layout")]
    public static void SaveEnemyBase()
    {
        EnemyBaseLayout layout = Resources.Load<EnemyBaseLayout>("EnemyBaseLayout");
        if (layout == null)
        {
            Debug.LogError("EnemyBaseLayout asset not found in Resources!");
            return;
        }

        layout.buildings.Clear();

        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (var go in allObjects)
        {
            if (PrefabUtility.GetPrefabAssetType(go) == PrefabAssetType.NotAPrefab)
                continue;

            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(go);
            if (prefab == null)
            {
                Debug.LogWarning($"Could not get prefab for {go.name}");
                continue;
            }

            layout.buildings.Add(new EnemyBaseLayout.BuildingPlacement
            {
                prefab = prefab,
                position = go.transform.position,
                rotationY = go.transform.rotation.eulerAngles.y
            });

            Debug.Log($"[Saved] {go.name} at {go.transform.position}");
        }

        EditorUtility.SetDirty(layout);
        AssetDatabase.SaveAssets();
        Debug.Log("[EnemyBaseSaver] Layout saved.");
    }

    // Removes all prefab instances from the current scene.
    [MenuItem("Tools/Clear Placed Enemy Buildings")]
    public static void ClearSceneBuildings()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        int count = 0;

        foreach (var go in allObjects)
        {
            if (PrefabUtility.GetPrefabAssetType(go) == PrefabAssetType.NotAPrefab)
                continue;

            GameObject.DestroyImmediate(go);
            count++;
        }

        Debug.Log($"[EnemyBaseSaver] Cleared {count} prefab instances from scene.");
    }
}

#endif