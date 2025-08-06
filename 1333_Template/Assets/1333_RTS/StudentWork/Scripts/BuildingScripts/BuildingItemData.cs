using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildItem", menuName = "Game/BuildItem")]

public class BuildingItemData : ScriptableObject
{

    public GameObject prefab;

    public string itemName;

    public ResourceType resourceCostType = ResourceType.Wood;
    public int resourceCost = 10;

    public int maxcount;
    public int requiredLevel;
    public Sprite icon;

    public BuildingPurpose purpose;

    public Vector2Int footprintSize = new Vector2Int(1, 1);
}


public enum BuildingPurpose { Defense, Resource, House, Extra }