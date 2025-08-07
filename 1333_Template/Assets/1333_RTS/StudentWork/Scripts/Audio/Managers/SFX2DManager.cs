// SFX2DManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 2D sound-effects manager with simple pooling.
/// Handles one-shot and looping 2D SFX, pooling, and basic volume control.
/// </summary>
public class SFX2DManager : AudioSingleton<SFX2DManager>
{
    /* ------------------------------------------------------------------ */
    /*  Inspector Settings                                                */
    /* ------------------------------------------------------------------ */
    [Header("2D Audio Settings")]
    public int maxPoolSize = 10;
    public AudioMixerGroup sfx2dMixerGroup;

    /* ------------------------------------------------------------------ */
    /*  Internal State                                                    */
    /* ------------------------------------------------------------------ */
    private readonly List<AudioSource> audioSourcePool = new List<AudioSource>();
    private readonly Dictionary<string, AudioSource> active2dSounds = new Dictionary<string, AudioSource>();

    /* ------------------------------------------------------------------ */
    /*  Unity Lifecycle                                                   */
    /* ------------------------------------------------------------------ */
    protected override void Awake()
    {
        base.Awake(); // Singleton initialization
        InitializePool();
    }

    /// <summary>
    /// Initialize the AudioSource pool for 2D sounds.
    /// </summary>
    private void InitializePool()
    {
        for (int i = 0; i < maxPoolSize; i++)
            CreateAudioSource("2D_AudioSource_" + i);
    }

    /// <summary>
    /// Create a pooled AudioSource for 2D SFX.
    /// </summary>
    private AudioSource CreateAudioSource(string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);

        AudioSource src = go.AddComponent<AudioSource>();
        src.spatialBlend = 0f;
        src.playOnAwake = false;
        src.loop = false;
        if (sfx2dMixerGroup != null)
            src.outputAudioMixerGroup = sfx2dMixerGroup;

        go.SetActive(false);
        audioSourcePool.Add(src);
        return src;
    }

    /// <summary>
    /// Retrieve an available AudioSource from the pool.
    /// </summary>
    private AudioSource GetAvailableAudioSource()
    {
        foreach (AudioSource src in audioSourcePool)
        {
            if (!src.isPlaying)
            {
                ResetAudioSource(src);
                return src;
            }
        }
        // Grow pool up to a soft limit if needed
        if (audioSourcePool.Count < maxPoolSize * 2)
        {
            AudioSource extra = CreateAudioSource("2D_AudioSource_Extra_" + audioSourcePool.Count);
            ResetAudioSource(extra);
            return extra;
        }
        Debug.LogWarning("SFX2DManager: No available 2D audio source in pool");
        return null;
    }

    /// <summary>
    /// Reset an AudioSource to default 2D settings.
    /// </summary>
    private static void ResetAudioSource(AudioSource src)
    {
        src.pitch = 1f;
        src.spatialBlend = 0f;
    }

    /* ------------------------------------------------------------------ */
    /*  Public API                                                        */
    /* ------------------------------------------------------------------ */

    /// <summary>
    /// Play a 2D one-shot sound effect.
    /// </summary>
    public void Play2dSfx(string soundName, AudioClip clip, float volume)
    {
        AudioSource src = GetAvailableAudioSource();
        if (src == null) return;

        src.clip = clip;
        src.volume = volume;
        src.loop = false;
        src.gameObject.SetActive(true);
        src.Play();

        string key = soundName + "_" + Time.time;
        active2dSounds[key] = src;
        StartCoroutine(DeactivateAfterDuration(key, clip.length));
    }

    /// <summary>
    /// Play or restart a looping 2D sound.
    /// </summary>
    public void Play2dLoop(string soundName,
                           AudioClip clip,
                           float volume,
                           bool restartIfPlaying = false)
    {
        // If already playing, restart or ignore based on flag
        if (active2dSounds.TryGetValue(soundName, out AudioSource current))
        {
            if (!current.isPlaying || restartIfPlaying)
            {
                current.Stop();
                current.clip = clip;
                current.volume = volume;
                current.loop = true;
                current.gameObject.SetActive(true);
                current.Play();
            }
            return; // Do not spawn a new one if already playing
        }

        // Get pooled source for new looping sound
        AudioSource src = GetAvailableAudioSource();
        if (src == null) return;

        src.clip = clip;
        src.volume = volume;
        src.loop = true;
        src.gameObject.SetActive(true);
        src.Play();

        // Store with plain name for later stop/volume changes
        active2dSounds[soundName] = src;
    }

    /// <summary>
    /// Stop all 2D sounds whose key starts with the given name prefix.
    /// </summary>
    public void Stop2dSound(string soundName)
    {
        List<string> toRemove = new List<string>();
        foreach (var kvp in active2dSounds)
        {
            if (kvp.Key.StartsWith(soundName))
            {
                AudioSource src = kvp.Value;
                src.Stop();
                src.gameObject.SetActive(false);
                toRemove.Add(kvp.Key);
            }
        }
        foreach (string key in toRemove)
            active2dSounds.Remove(key);
    }

    /// <summary>
    /// Stop all currently playing 2D sounds.
    /// </summary>
    public void StopAll2dSounds()
    {
        foreach (var kvp in active2dSounds)
        {
            kvp.Value.Stop();
            kvp.Value.gameObject.SetActive(false);
        }
        active2dSounds.Clear();
    }

    /// <summary>
    /// Set the volume for a specific 2D sound by name.
    /// </summary>
    public void Set2dSoundVolume(string soundName, float volume)
    {
        if (active2dSounds.TryGetValue(soundName, out AudioSource src))
            src.volume = volume;
        else
            Debug.LogWarning("SFX2DManager: No active sound '" + soundName + "'");
    }

    /// <summary>
    /// Adjust the 2D SFX mixer group volume using a slider value (0-1).
    /// </summary>
    public void Adjust2DAudioMixerVolume(float sliderValue)
    {
        if (sfx2dMixerGroup != null && sfx2dMixerGroup.audioMixer != null)
            sfx2dMixerGroup.audioMixer.SetFloat("Volume", AudioUtils.SliderToDb(sliderValue));
    }

    /* ------------------------------------------------------------------ */
    /*  Internal Helpers                                                  */
    /* ------------------------------------------------------------------ */

    /// <summary>
    /// Coroutine: deactivate and return source to pool after duration.
    /// </summary>
    private IEnumerator DeactivateAfterDuration(string key, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (active2dSounds.TryGetValue(key, out AudioSource src))
        {
            src.Stop();
            src.gameObject.SetActive(false);
            active2dSounds.Remove(key);
        }
    }
}
