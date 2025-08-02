using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Central background music controller:
/// - Enum-based track lookup
/// - Crossfade playback
/// - Runtime volume and mixer control
/// </summary>
public class MusicManager : AudioSingleton<MusicManager>
{
    /* ------------------------------------------------------------------ */
    /*  Inspector Settings                                                */
    /* ------------------------------------------------------------------ */
    [Header("Music Settings")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioMixerGroup _musicMixerGroup;

    [Header("Music clip database")]
    [SerializeField] private MusicTrackDatabase _database = null;

    /* ------------------------------------------------------------------ */
    /*  Internal State                                                    */
    /* ------------------------------------------------------------------ */
    private Dictionary<MusicTrack, AudioClip> _musicDictionary;
    private Coroutine _fadeRoutine;
    private float _targetVolume = 0.5f;

    /* ------------------------------------------------------------------ */
    /*  Unity Lifecycle                                                   */
    /* ------------------------------------------------------------------ */
    protected override void Awake()
    {
        base.Awake(); // Ensures safe singleton initialization

        // Build or assign the AudioSource for music playback
        if (_musicSource == null)
        {
            GameObject obj = new GameObject("MusicSource");
            obj.transform.SetParent(transform, false);
            _musicSource = obj.AddComponent<AudioSource>();
            _musicSource.spatialBlend = 0f;
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.bypassReverbZones = true;

            if (_musicMixerGroup != null)
                _musicSource.outputAudioMixerGroup = _musicMixerGroup;
        }

        // Convert the music list to a dictionary for fast lookup
        _musicDictionary = new Dictionary<MusicTrack, AudioClip>(_database.Entries.Count);
        foreach (var e in _database.Entries)
            if (!_musicDictionary.ContainsKey(e.trackType))
            _musicDictionary.Add(e.trackType, e.clip);
    }

    /* ------------------------------------------------------------------ */
    /*  Public API                                                        */
    /* ------------------------------------------------------------------ */

    /// <summary>
    /// Play a music track by enum, with optional crossfade.
    /// </summary>
    /// <param name="track">Track to play</param>
    /// <param name="volume">Target volume (0-1)</param>
    /// <param name="loop">Whether to loop</param>
    /// <param name="fadeTime">Crossfade duration (seconds)</param>
    public void PlayMusicByEnum(MusicTrack track,
                                float volume = 0.5f,
                                bool loop = true,
                                float fadeTime = 0.5f)
    {
        _targetVolume = Mathf.Clamp01(volume);

        if (_musicDictionary.TryGetValue(track, out var clip))
        {
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(CrossfadeRoutine(clip, loop, fadeTime));
        }
        else
        {
            Debug.LogWarning($"MusicManager: Track not found {track}");
        }
    }

    /// <summary>
    /// Fade out and stop the music.
    /// </summary>
    /// <param name="fadeTime">Fade-out duration (seconds)</param>
    public void StopMusic(float fadeTime = 0.3f)
    {
        if (!_musicSource.isPlaying) return;

        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeVolumeRoutine(0f, fadeTime, () =>
        {
            _musicSource.Stop();
            _musicSource.clip = null;
        }));
    }

    /// <summary>
    /// Fade the music to a specific volume over time.
    /// </summary>
    /// <param name="volume">Target volume (0-1)</param>
    /// <param name="time">Fade duration (seconds)</param>
    public void FadeToVolume(float volume, float time = 0.3f)
    {
        _targetVolume = Mathf.Clamp01(volume);

        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeVolumeRoutine(_targetVolume, time));
    }

    /// <summary>
    /// Set the music mixer group volume directly via slider (0-1).
    /// </summary>
    /// <param name="sliderValue">Slider value (0-1)</param>
    public void SetMixerVolume(float sliderValue)
    {
        if (_musicMixerGroup == null || _musicMixerGroup.audioMixer == null) return;
        _musicMixerGroup.audioMixer.SetFloat("Volume", AudioUtils.SliderToDb(sliderValue));
    }

    /* ------------------------------------------------------------------ */
    /*  Private Helpers                                                   */
    /* ------------------------------------------------------------------ */

    /// <summary>
    /// Coroutine to crossfade from current to new music clip.
    /// </summary>
    private IEnumerator CrossfadeRoutine(AudioClip newClip, bool loop, float time)
    {
        // Fade out current music if playing
        if (_musicSource.isPlaying)
            yield return FadeVolumeRoutine(0f, time * 0.5f);

        // Switch to new music clip
        _musicSource.clip = newClip;
        _musicSource.loop = loop;
        _musicSource.Play();

        // Fade in to target volume
        yield return FadeVolumeRoutine(_targetVolume, time * 0.5f);
    }

    /// <summary>
    /// Coroutine to smoothly fade volume to the target value.
    /// </summary>
    private IEnumerator FadeVolumeRoutine(float target, float time, System.Action onComplete = null)
    {
        float start = _musicSource.volume;
        float elapsed = 0f;

        while (elapsed < time)
        {
            elapsed += Time.unscaledDeltaTime;
            _musicSource.volume = Mathf.Lerp(start, target, elapsed / time);
            yield return null;
        }

        _musicSource.volume = target;
        onComplete?.Invoke();
    }
}
