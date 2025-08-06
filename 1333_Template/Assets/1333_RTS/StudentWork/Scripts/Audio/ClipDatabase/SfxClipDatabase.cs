using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Named SFX clip lookup table.
/// </summary>
[CreateAssetMenu(menuName = "Audio System/SFX Clip Database", fileName = "Sfx Clip Database")]
public class SfxClipDatabase : ScriptableObject
{
    [SerializeField] private List<AudioClip> _clips = new();
    public IReadOnlyList<AudioClip> clips => _clips;

    private Dictionary<string, AudioClip> _dict;

    private void OnEnable()
    {
        _dict = new Dictionary<string, AudioClip>(_clips.Count);
        foreach (var c in _clips)
            if (!_dict.ContainsKey(c.name)) _dict.Add(c.name, c);
    }

    public bool TryGetClip(string name, out AudioClip clip) =>
        _dict.TryGetValue(name, out clip);
}
