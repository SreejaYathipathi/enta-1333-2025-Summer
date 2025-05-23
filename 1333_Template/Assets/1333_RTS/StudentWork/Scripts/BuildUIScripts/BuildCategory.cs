using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildCategory", menuName = "Game/BuildCategory")]

public class BuildCategory : ScriptableObject
{
    public string categoryName;
    public List<BuildItemData> items;
}
