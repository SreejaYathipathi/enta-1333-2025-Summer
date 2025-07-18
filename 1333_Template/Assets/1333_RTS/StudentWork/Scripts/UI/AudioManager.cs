using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip hoverSound;
    public AudioClip menuClickSound;
    public AudioClip buildingClickSound;

    [HideInInspector] public float uiVolume = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            musicSource.loop = true;
            PlayMusic();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMusic()
    {
        if (backgroundMusic)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip, uiVolume);
    }

    public void PlayHover() => PlaySFX(hoverSound, uiVolume);
    public void PlayMenuClick() => PlaySFX(menuClickSound, uiVolume);
    public void PlayBuildingClick() => PlaySFX(buildingClickSound, uiVolume);

    public void SetMusicVolume(float value)
    {
        musicSource.volume = value;
    }

    public void SetSFXVolume(float value)
    {
        sfxSource.volume = value;
    }

    public void SetUIVolume(float value)
    {
        uiVolume = value;
    }
}
