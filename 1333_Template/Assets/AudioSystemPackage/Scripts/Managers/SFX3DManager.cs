// SFX3DManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 3D sound-effects manager with pooling and distance-based spatial blend.
/// Handles one-shot and looping 3D SFX, spatialization, and pooling.
/// </summary>
public class SFX3DManager : AudioSingleton<SFX3DManager>
{
    /* ------------------------------------------------------------------ */
    /*  Inspector Settings                                                */
    /* ------------------------------------------------------------------ */
    [Header("3D Audio Settings")]
    public int maxPoolSize = 10;
    public float defaultMinDistance = 1f;
    public float defaultMaxDistance = 20f;
    public float defaultDopplerLevel = 1f;
    public float defaultSpread = 0f;
    public AudioMixerGroup sfx3dMixerGroup;

    /* ------------------------------------------------------------------ */
    /*  Internal State                                                    */
    /* ------------------------------------------------------------------ */
    private readonly List<AudioSource> audioSourcePool = new List<AudioSource>();
    private readonly Dictionary<string, AudioSource> active3dSounds = new Dictionary<string, AudioSource>();
    private Camera cachedCamera;

#if UNITY_EDITOR
    /// <summary>
    /// Cleanup for pooled AudioSources and manager object when exiting Play Mode in the Editor.
    /// </summary>
    private void OnDestroy()
    {
        if (!Application.isPlaying)
        {
            // Destroy all pooled AudioSource GameObjects
            for (int i = transform.childCount - 1; i >= 0; --i)
                DestroyImmediate(transform.GetChild(i).gameObject);
            // Destroy the manager GameObject itself
            DestroyImmediate(gameObject);
        }
    }
#endif

    /* ------------------------------------------------------------------ */
    /*  Unity Lifecycle                                                   */
    /* ------------------------------------------------------------------ */
    protected override void Awake()
    {
        base.Awake(); // Singleton initialization

        cachedCamera = Camera.main;
        if (cachedCamera == null)
            Debug.LogWarning("SFX3DManager: No main camera found. Spatial blend adjustment may not work.");

        InitializePool();
    }

    /// <summary>
    /// Initialize the AudioSource pool for 3D sounds.
    /// </summary>
    private void InitializePool()
    {
        for (int i = 0; i < maxPoolSize; i++)
            CreateAudioSource("3D_AudioSource_" + i);
    }

    /// <summary>
    /// Create a pooled AudioSource for 3D SFX.
    /// </summary>
    private AudioSource CreateAudioSource(string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);

        AudioSource src = go.AddComponent<AudioSource>();
        src.spatialBlend = 1f;
        src.playOnAwake = false;
        src.loop = false;
        src.rolloffMode = AudioRolloffMode.Logarithmic;
        src.minDistance = defaultMinDistance;
        src.maxDistance = defaultMaxDistance;
        if (sfx3dMixerGroup != null)
            src.outputAudioMixerGroup = sfx3dMixerGroup;

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

        // Optionally grow pool up to a limit if needed
        if (audioSourcePool.Count < maxPoolSize * 2)
        {
            AudioSource extra = CreateAudioSource("3D_AudioSource_Extra_" + audioSourcePool.Count);
            ResetAudioSource(extra);
            return extra;
        }

        Debug.LogWarning("SFX3DManager: No available 3D audio source in pool");
        return null;
    }

    /// <summary>
    /// Reset an AudioSource to default 3D settings.
    /// </summary>
    private void ResetAudioSource(AudioSource src)
    {
        src.pitch = 1f;
        src.spatialBlend = 1f;
        src.dopplerLevel = defaultDopplerLevel;
        src.spread = defaultSpread;
        src.rolloffMode = AudioRolloffMode.Logarithmic;
        src.minDistance = defaultMinDistance;
        src.maxDistance = defaultMaxDistance;
    }

    /* ------------------------------------------------------------------ */
    /*  Public API                                                        */
    /* ------------------------------------------------------------------ */

    /// <summary>
    /// Play a 3D one-shot sound at the specified transform.
    /// </summary>
    public void Play3dSfx(string soundName,
                          AudioClip clip,
                          Transform spawn,
                          float volume,
                          float? minDist = null,
                          float? maxDist = null)
    {
        InternalPlay3d(soundName, clip, spawn, volume,
                       loop: false, minDist, maxDist, restartIfPlaying: false);
    }

    /// <summary>
    /// Play or restart a looping 3D sound at the specified transform.
    /// </summary>
    public void Play3dLoop(string soundName,
                           AudioClip clip,
                           Transform spawn,
                           float volume,
                           bool restartIfPlaying = false,
                           float? minDist = null,
                           float? maxDist = null)
    {
        InternalPlay3d(soundName, clip, spawn, volume,
                       loop: true, minDist, maxDist, restartIfPlaying);
    }

    /// <summary>
    /// Play looping ambience with automatic spatial blend adjustment over distance.
    /// </summary>
    public void Play3dAmbience(string soundName, AudioClip clip, Transform spawn, float volume, float minDistance = 1f, float maxDistance = 50f)
    {
        Play3dLoop(soundName, clip, spawn, volume, false, minDistance, maxDistance);

        if (active3dSounds.TryGetValue(soundName, out AudioSource ambience))
        {
            ambience.dopplerLevel = 0f;
            StartCoroutine(AdjustSpatialBlendOverDistance(ambience, minDistance, maxDistance));
        }
    }

    /// <summary>
    /// Stop all sounds whose key starts with the given name prefix.
    /// </summary>
    public void Stop3dSound(string soundName)
    {
        List<string> toRemove = new List<string>();
        foreach (var kvp in active3dSounds)
        {
            if (kvp.Key.StartsWith(soundName))
            {
                AudioSource src = kvp.Value;
                src.Stop();
                src.transform.SetParent(transform, false);
                src.gameObject.SetActive(false);
                toRemove.Add(kvp.Key);
            }
        }
        foreach (string key in toRemove)
            active3dSounds.Remove(key);
    }

    /// <summary>
    /// Stop all currently playing 3D sounds.
    /// </summary>
    public void StopAll3dSounds()
    {
        foreach (var kvp in active3dSounds)
        {
            kvp.Value.Stop();
            kvp.Value.transform.SetParent(transform, false);
            kvp.Value.gameObject.SetActive(false);
        }
        active3dSounds.Clear();
    }

    /// <summary>
    /// Set the volume for a specific 3D sound.
    /// </summary>
    public void Set3dSoundVolume(string soundName, float volume)
    {
        if (active3dSounds.TryGetValue(soundName, out AudioSource src))
            src.volume = volume;
        else
            Debug.LogWarning("SFX3DManager: No active sound '" + soundName + "'");
    }

    /// <summary>
    /// Adjust the 3D SFX mixer group volume using a slider value (0-1).
    /// </summary>
    public void Adjust3DAudioMixerVolume(float sliderValue)
    {
        if (sfx3dMixerGroup != null && sfx3dMixerGroup.audioMixer != null)
            sfx3dMixerGroup.audioMixer.SetFloat("Volume", AudioUtils.SliderToDb(sliderValue));
    }

    /* ------------------------------------------------------------------ */
    /*  Internal Helpers                                                  */
    /* ------------------------------------------------------------------ */

    /// <summary>
    /// Internal logic for pooled 3D audio playback and registration.
    /// </summary>
    private void InternalPlay3d(string soundName,
                                AudioClip clip,
                                Transform spawn,
                                float volume,
                                bool loop,
                                float? minDist,
                                float? maxDist,
                                bool restartIfPlaying)
    {
        // Handle looping: restart or ignore if already playing
        if (loop && active3dSounds.TryGetValue(soundName, out AudioSource playing))
        {
            if (!playing.isPlaying || restartIfPlaying)
            {
                playing.Stop();
                ResetAudioSource(playing);
                ConfigureAndPlay(playing, clip, spawn, volume,
                                 loop, minDist, maxDist);
            }
            return;
        }

        // Get an available source from the pool
        AudioSource src = GetAvailableAudioSource();
        if (src == null) return;

        ConfigureAndPlay(src, clip, spawn, volume,
                         loop, minDist, maxDist);

        // Register source in active dictionary
        if (loop)
        {
            active3dSounds[soundName] = src;
        }
        else
        {
            string key = $"{soundName}_{Time.time}";
            active3dSounds[key] = src;
            StartCoroutine(DeactivateAfterDuration(key, clip.length));
        }
    }

    /// <summary>
    /// Configure and play an AudioSource with given 3D settings.
    /// </summary>
    private void ConfigureAndPlay(AudioSource src,
                                  AudioClip clip,
                                  Transform spawn,
                                  float volume,
                                  bool loop,
                                  float? minDist,
                                  float? maxDist)
    {
        float minD = minDist ?? defaultMinDistance;
        float maxD = maxDist ?? defaultMaxDistance;

        src.transform.SetParent(spawn, false);
        src.transform.localPosition = Vector3.zero;

        src.clip = clip;
        src.volume = volume;
        src.loop = loop;
        src.minDistance = minD;
        src.maxDistance = maxD;

        src.gameObject.SetActive(true);
        src.Play();
    }

    /// <summary>
    /// Coroutine: deactivate one-shot audio sources after they finish playing.
    /// </summary>
    private IEnumerator DeactivateAfterDuration(string key, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (active3dSounds.TryGetValue(key, out AudioSource src))
        {
            src.Stop();
            src.transform.SetParent(transform, false);
            src.gameObject.SetActive(false);
            active3dSounds.Remove(key);
        }
    }

    /// <summary>
    /// Coroutine: adjust spatial blend based on distance from camera.
    /// </summary>
    private IEnumerator AdjustSpatialBlendOverDistance(AudioSource src, float minDistance, float maxDistance)
    {
        while (src != null && src.isPlaying)
        {
            float dist = Vector3.Distance(src.transform.position, cachedCamera.transform.position);
            src.spatialBlend = (dist <= minDistance) ? 0f : Mathf.Clamp01((dist - minDistance) / (maxDistance - minDistance));
            yield return new WaitForSeconds(0.2f);
        }
    }
}
