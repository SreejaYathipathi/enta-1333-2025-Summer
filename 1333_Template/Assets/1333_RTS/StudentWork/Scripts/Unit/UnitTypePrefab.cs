using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class UnitTypePrefab
{
    // Reference to the unit's data (size, stats, etc.)
    public UnitType unitType;
    // Reference to the prefab for this unit type
    public GameObject prefab;
}
