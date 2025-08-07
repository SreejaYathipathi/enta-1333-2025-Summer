// LevelTable.cs  (overwrite the old ScriptableObject)
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Level Table (Formula)")]
public class LevelTable : ScriptableObject
{
    [Header("Curve")]
    [Min(1)] public int maxLevel = 50;
    [Min(1)] public int baseXP = 100;
    [Range(0f, 1f)] public float earlyGrowth = 0.15f;
    [Range(0f, 1f)] public float lateGrowth = 0.25f;

    public int GetXpForLevel(int level)
    {
        if (level <= 1) return 0;

        float xp = baseXP;
        for (int lv = 3; lv <= level; lv++)
        {
            float growth = lv <= 10 ? earlyGrowth : lateGrowth;
            xp += xp * growth;
        }
        return Mathf.RoundToInt(xp);
    }

    public int MaxLevel => maxLevel;
}
