using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildItem", menuName = "Game/BuildItem")]

public class BuildItemData : ScriptableObject
{

    public GameObject previewPrefab;
    public GameObject actualPrefab;

    public string itemName;
    public int cost;
    public int maxcount;
    public int requiredLevel;
    public Sprite icon;
}
