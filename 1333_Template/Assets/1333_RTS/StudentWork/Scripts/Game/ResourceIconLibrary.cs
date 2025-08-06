using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceIconLibrary",
                 menuName = "Game/Resource Icon Library")]
public class ResourceIconLibrary : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public ResourceType type;   // Wood, Stone, …
        public Sprite icon;
    }

    [SerializeField] private List<Entry> icons = new();

    /* quick lookup at runtime */
    private Dictionary<ResourceType, Sprite> _dict;

    void OnEnable()
    {
        _dict = new Dictionary<ResourceType, Sprite>();
        foreach (var e in icons)
            if (!_dict.ContainsKey(e.type) && e.icon)
                _dict.Add(e.type, e.icon);
    }

    public Sprite GetIcon(ResourceType type)
    {
        return _dict != null && _dict.TryGetValue(type, out var s) ? s : null;
    }
}
