using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ArmyComposition", menuName = "Game/Army Composition")]
public class ArmyComposition : ScriptableObject
{
    [System.Serializable]
    public class UnitEntry
    {
        public UnitTypePrefab unitTypePrefab;
        public int count = 1;
    }

    public List<UnitEntry> units = new();
}
