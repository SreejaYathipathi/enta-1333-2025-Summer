using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Central audio manager: handles clip database, mixer control, and volume sliders.
/// </summary>
public class AudioManager : AudioSingleton<AudioManager>
{
    /* ------------------------------------------------------------------ */
    /*  Mixer Group References                                            */
    /* ------------------------------------------------------------------ */
    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup masterMixer;
    [SerializeField] private AudioMixerGroup musicMixer;
    [SerializeField] private AudioMixerGroup sfxMixer;

    /* ------------------------------------------------------------------ */
    /*  Audio Clip Database (Optional)                                    */
    /* ------------------------------------------------------------------ */
    [Header("SFX Clip Database")]
    [SerializeField] private SfxClipDatabase _sfxDatabase = null;

    private readonly Dictionary<string, AudioClip> sfxDict = new Dictionary<string, AudioClip>();

    /* ------------------------------------------------------------------ */
    /*  Cached Volume Slider Values (0‑1 Range)                           */
    /* ------------------------------------------------------------------ */
    private float masterVol = 1f;
    private float musicVol = 1f;
    private float sfxVol = 1f;

    /* ------------------------------------------------------------------ */
    /*  Unity Lifecycle                                                   */
    /* ------------------------------------------------------------------ */
    protected override void Awake()
    {
        base.Awake();          // Ensures safe singleton initialization
        CacheClips();          // Build lookup dictionaries for audio clips
        LoadVolumes();         // Load saved volume slider values
        ApplyAllVolumes();     // Apply loaded volumes to the mixer
    }

    /* ------------------------------------------------------------------ */
    /*  Audio Clip Helpers                                                */
    /* ------------------------------------------------------------------ */
    /// <summary>
    /// Build fast lookup dictionary for all SFX clips from the database.
    /// </summary>
    private void CacheClips()
    {
        if (_sfxDatabase == null) return;

        IReadOnlyList<AudioClip> clips = _sfxDatabase.clips;
        for (int i = 0; i < clips.Count; i++)
        {
            AudioClip clip = clips[i];
            if (clip != null && !sfxDict.ContainsKey(clip.name))
            {
                sfxDict.Add(clip.name, clip);
            }
        }
    }

    /// <summary>
    /// Retrieve an SFX AudioClip by name.
    /// </summary>
    public AudioClip GetSfx(string clipName) 
    { 
        if(sfxDict.TryGetValue(clipName, out var c))
        {
            return c;
        }
        else
        {
            Debug.LogWarning($"{clipName} is not in SFX dictionary. Check sfx name again.");
            return null;
        }
    }

    /* ------------------------------------------------------------------ */
    /*  Volume Control                                                    */
    /* ------------------------------------------------------------------ */
    /// <summary>
    /// Set the mixer volume for a given channel (0‑1 range).
    /// </summary>
    public void SetVolume(AudioChannel ch, float value)
    {
        float db = AudioUtils.SliderToDb(Mathf.Clamp01(value));

        switch (ch)
        {
            case AudioChannel.Master:
                masterMixer.audioMixer.SetFloat("VolumeMaster", db);
                masterVol = value;
                break;
            case AudioChannel.Music:
                musicMixer.audioMixer.SetFloat("VolumeMusic", db);
                musicVol = value;
                break;
            case AudioChannel.Sfx:
                sfxMixer.audioMixer.SetFloat("VolumeSFX", db);
                sfxVol = value;
                break;
        }
    }

    /// <summary>
    /// Apply all cached volume values to the mixer.
    /// </summary>
    private void ApplyAllVolumes()
    {
        SetVolume(AudioChannel.Master, masterVol);
        SetVolume(AudioChannel.Music, musicVol);
        SetVolume(AudioChannel.Sfx, sfxVol);
    }

    /* ------------------------------------------------------------------ */
    /*  UI Slider Callbacks                                               */
    /* ------------------------------------------------------------------ */
    /// <summary>
    /// Get the cached volume value (0-1) for a given channel.
    /// Useful for initializing UI sliders.
    /// </summary>
    public float GetVolume01(AudioChannel ch)
    {
        switch (ch)
        {
            case AudioChannel.Master: return masterVol;
            case AudioChannel.Music: return musicVol;
            case AudioChannel.Sfx: return sfxVol;
            default: return 1f;
        }
    }

    /// <summary>
    /// Master volume slider UI callback.
    /// </summary>
    public void OnMasterSlider(float value) => SetVolume(AudioChannel.Master, value);

    /// <summary>
    /// Music volume slider UI callback.
    /// </summary>
    public void OnMusicSlider(float value) => SetVolume(AudioChannel.Music, value);

    /// <summary>
    /// SFX volume slider UI callback.
    /// </summary>
    public void OnSfxSlider(float value) => SetVolume(AudioChannel.Sfx, value);

    /* ------------------------------------------------------------------ */
    /*  Persistent Volume Save / Load                                     */
    /* ------------------------------------------------------------------ */
    private const string KeyMaster = "Audio_Master";
    private const string KeyMusic = "Audio_Music";
    private const string KeySfx = "Audio_Sfx";

    /// <summary>
    /// Save all volume settings to PlayerPrefs.
    /// </summary>
    public void SaveVolumes()
    {
        PlayerPrefs.SetFloat(KeyMaster, masterVol);
        PlayerPrefs.SetFloat(KeyMusic, musicVol);
        PlayerPrefs.SetFloat(KeySfx, sfxVol);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load all volume settings from PlayerPrefs.
    /// </summary>
    private void LoadVolumes()
    {
        masterVol = PlayerPrefs.GetFloat(KeyMaster, 1f);
        musicVol = PlayerPrefs.GetFloat(KeyMusic, 1f);
        sfxVol = PlayerPrefs.GetFloat(KeySfx, 1f);
    }

    /// <summary>
    /// Save volumes when the application is quitting.
    /// </summary>
    private void OnApplicationQuit() => SaveVolumes();

    /* ================================================================== */
    /*  PUBLIC WRAPPERS FOR SFX / MUSIC MANAGERS                          */
    /*  (Add inside AudioManager class)                                   */
    /* ================================================================== */

    #region SFX‑2D WRAPPERS
    /// <summary>
    /// Play a 2D one-shot sound by clip name (uses internal clip database).
    /// </summary>
    public void Play2dSfx(string soundName, float volume = 1f)
    {
        AudioClip clip = GetSfx(soundName);
        if (clip != null && SFX2DManager.Instance != null)
            SFX2DManager.Instance.Play2dSfx(soundName, clip, volume);
    }

    /// <summary>
    /// Play or restart a looping 2D sound.
    /// </summary>
    public void Play2dLoop(string soundName,
                           float volume = 1f,
                           bool restartIfPlaying = false)
    {
        AudioClip clip = GetSfx(soundName);
        if (clip != null && SFX2DManager.Instance != null)
            SFX2DManager.Instance.Play2dLoop(soundName, clip, volume, restartIfPlaying);
    }

    /// <summary>
    /// Stop a specific 2D sound (prefix match by name).
    /// </summary>
    public void Stop2dSound(string soundName) =>
        SFX2DManager.Instance?.Stop2dSound(soundName);

    /// <summary>
    /// Stop all playing 2D sounds.
    /// </summary>
    public void StopAll2dSounds() =>
        SFX2DManager.Instance?.StopAll2dSounds();

    /// <summary>
    /// Change volume of a running 2D sound.
    /// </summary>
    public void Set2dSoundVolume(string soundName, float volume) =>
        SFX2DManager.Instance?.Set2dSoundVolume(soundName, volume);
    #endregion

    #region SFX‑3D WRAPPERS
    /// <summary>
    /// Play a 3D one-shot sound at the given transform.
    /// </summary>
    public void Play3dSfx(string soundName,
                          Transform spawn,
                          float volume = 1f,
                          float? minDist = null,
                          float? maxDist = null)
    {
        AudioClip clip = GetSfx(soundName);
        if (clip != null && SFX3DManager.Instance != null)
            SFX3DManager.Instance.Play3dSfx(soundName, clip, spawn,
                                            volume, minDist, maxDist);
    }

    /// <summary>
    /// Play or restart a looping 3D sound at the given transform.
    /// </summary>
    public void Play3dLoop(string soundName,
                           Transform spawn,
                           float volume = 1f,
                           bool restartIfPlaying = false,
                           float? minDist = null,
                           float? maxDist = null)
    {
        AudioClip clip = GetSfx(soundName);
        if (clip != null && SFX3DManager.Instance != null)
            SFX3DManager.Instance.Play3dLoop(soundName, clip, spawn, volume,
                                             restartIfPlaying, minDist, maxDist);
    }

    /// <summary>
    /// Stop a specific 3D sound (prefix match by name).
    /// </summary>
    public void Stop3dSound(string soundName) =>
        SFX3DManager.Instance?.Stop3dSound(soundName);

    /// <summary>
    /// Stop all playing 3D sounds.
    /// </summary>
    public void StopAll3dSounds() =>
        SFX3DManager.Instance?.StopAll3dSounds();

    /// <summary>
    /// Change volume of a running 3D sound.
    /// </summary>
    public void Set3dSoundVolume(string soundName, float volume) =>
        SFX3DManager.Instance?.Set3dSoundVolume(soundName, volume);
    #endregion

    #region MUSIC WRAPPERS
    /// <summary>
    /// Play a background music track with crossfade.
    /// </summary>
    public void PlayMusic(MusicTrack track,
                          float volume = 0.5f,
                          bool loop = true,
                          float fadeTime = 0.5f) =>
        MusicManager.Instance?.PlayMusicByEnum(track, volume, loop, fadeTime);

    /// <summary>
    /// Fade out and stop the current music track.
    /// </summary>
    public void StopMusic(float fadeTime = 0.3f) =>
        MusicManager.Instance?.StopMusic(fadeTime);

    /// <summary>
    /// Fade the current music track to a new volume (0-1 range).
    /// </summary>
    public void FadeMusicTo(float volume, float time = 0.3f) =>
        MusicManager.Instance?.FadeToVolume(volume, time);

    /// <summary>
    /// Set the mixer volume for the music group directly (slider value 0-1).
    /// </summary>
    public void SetMusicMixerVolume(float sliderValue) =>
        MusicManager.Instance?.SetMixerVolume(sliderValue);
    #endregion
    /* ================================================================== */
}

/// <summary>
/// Enum for logical audio mixer channels.
/// </summary>
public enum AudioChannel { Master, Music, Sfx }
