using System.Collections.Generic;
using UnityEngine;


// This attribute allows you to create instances of UnitType as assets via the Unity Editor's "Create" menu
[CreateAssetMenu(fileName = "BuildCategory", menuName = "Game/BuildCategory")]

public class BuildingCategory : ScriptableObject
{
    public string categoryName;
    public List<BuildingItemData> items;
}
