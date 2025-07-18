using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("MainMenu")]
    public GameObject mainMenuPanel;
    public GameObject mainSettingsPanel;
    public GameObject mainControlsPanel;
    public GameObject gameName;

    [Header("PauseMenu")]
    public GameObject pauseMenuPanel;
    public GameObject pauseSettingsPanel;
    public GameObject pauseControlsPanel;
    public CameraController cameraController;

    [Header("Volume Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;

    private bool _isPaused = false;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        cameraController.enabled = false;
        Time.timeScale = 0f;
        _isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        cameraController.enabled = true;
        Time.timeScale = 1f;
        _isPaused = false;
    }

    public void OpenMainSettings()
    {
        mainSettingsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        mainControlsPanel.SetActive(false);
        gameName.SetActive(false);

        SetupVolumeSliders();
    }

    public void OpenPauseSettings()
    {
        pauseSettingsPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);
        pauseControlsPanel.SetActive(false);
        cameraController.enabled = false;

        Time.timeScale = 0f;
        _isPaused = true;

        SetupVolumeSliders();
    }

    private void SetupVolumeSliders()
    {
        musicSlider.value = AudioManager.Instance.musicSource.volume;
        sfxSlider.value = AudioManager.Instance.sfxSource.volume;

        musicSlider.onValueChanged.RemoveAllListeners();
        musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);

        sfxSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
    }
    public void OpenMainControls()
    {
        mainControlsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        mainSettingsPanel.SetActive(false);
        gameName.SetActive(false);
    }

    public void OpenPauseControls()
    {
        pauseControlsPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);
        pauseSettingsPanel.SetActive(false);
        cameraController.enabled = false;

        Time.timeScale = 0f;
        _isPaused = true;
    }

    public void MainBack()
    {
        mainSettingsPanel.SetActive(false);
        mainControlsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        gameName.SetActive(true);
    }

    public void PauseBack()
    {
        pauseControlsPanel.SetActive(false);
        pauseSettingsPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
        cameraController.enabled = false;

        Time.timeScale = 0f;
        _isPaused = true;
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }
}
