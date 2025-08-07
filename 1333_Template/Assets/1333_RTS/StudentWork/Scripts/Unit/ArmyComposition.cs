using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ArmyComposition", menuName = "Game/Army Composition")]
public class ArmyComposition : ScriptableObject
{
    // Holds one line of the roster: prefab reference + quantity.
    [System.Serializable]
    public class UnitEntry
    {
        public UnitTypePrefab unitTypePrefab;
        public int count = 1;
    }

    // The full ordered list for this army preset.
    public List<UnitEntry> units = new();
}
