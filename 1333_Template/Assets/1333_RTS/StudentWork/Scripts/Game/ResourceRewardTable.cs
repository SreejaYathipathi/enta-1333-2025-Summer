using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceRewardTable",
                 menuName = "Game/Resource Reward Table")]
public class ResourceRewardTable : ScriptableObject
{
    [System.Serializable]
    public struct Row
    {
        public ResourceType type;
        [Range(1, 100)] public int minAmount;
        [Range(1, 999)] public int maxAmount;
        [Range(1, 100)] public int weight;    // higher = more common
    }

    public List<Row> rows = new();

    /* quick lookup */
    private Dictionary<ResourceType, Row> _dict;
    private int _totalWeight;

    void OnEnable()
    {
        _dict = new Dictionary<ResourceType, Row>();
        _totalWeight = 0;
        foreach (var r in rows)
        {
            _dict[r.type] = r;
            _totalWeight += r.weight;
        }
    }

    public bool TryGetRow(ResourceType type, out Row row) =>
        _dict.TryGetValue(type, out row);

    /* pick one type based on weights */
    public ResourceType GetRandomType()
    {
        int roll = Random.Range(1, _totalWeight + 1);
        int acc = 0;
        foreach (var r in rows)
        {
            acc += r.weight;
            if (roll <= acc) return r.type;
        }
        return rows[0].type;  // fallback
    }
}
