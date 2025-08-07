using System.Collections.Generic;
using UnityEngine;

// ScriptableObject that stores a list of building placements for a pre-made enemy base.
[CreateAssetMenu(fileName = "EnemyBaseLayout", menuName = "Game/Enemy Base Layout")]
public class EnemyBaseLayout : ScriptableObject
{
    [System.Serializable]
   public class BuildingPlacement
    {
        public GameObject prefab;
        public Vector3 position;
        public float rotationY;
        public Vector2Int footprintSize;
    }

    public List<BuildingPlacement> buildings = new();
}