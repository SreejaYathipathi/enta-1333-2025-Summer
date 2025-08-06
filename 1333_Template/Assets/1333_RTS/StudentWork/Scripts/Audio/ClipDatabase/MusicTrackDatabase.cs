using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Asset that maps MusicTrack enum to AudioClip.
/// </summary>
[CreateAssetMenu(menuName = "Audio System/Music Track Database", fileName = "MusicTrack Data Base")]
public class MusicTrackDatabase : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public MusicTrack trackType;
        public AudioClip clip;
    }

    [SerializeField] private List<Entry> _entries = new();

    public IReadOnlyList<Entry> Entries => _entries;
}
