using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    [Header("Game Over")]
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Loading")]
    public GameObject loadingPanel;

    [Header("Player Profile")]
    public Image playerProfileImage;
    public TMP_Text playerNameText;
    public List<Sprite> profileImages;

    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance.CurrentState == GameState.Paused)
                ResumeGame();
            else if (GameManager.Instance.CurrentState == GameState.Gameplay)
                PauseGame();
        }
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState state)
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(state == GameState.Loading);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(state == GameState.Paused);

        if (state == GameState.Gameplay)
            UpdatePlayerProfile();
    }

    private void UpdatePlayerProfile()
    {
        int slot = PlayerPrefs.GetInt("LastUsedSlot", -1);
        if (slot == -1) return;

        string name = SaveManager.GetSlotName(slot);
        int imageIndex = SaveManager.GetProfileImageIndex(slot);

        if (playerNameText != null)
            playerNameText.text = string.IsNullOrEmpty(name) ? "Player" : name;

        if (playerProfileImage != null && imageIndex >= 0 && imageIndex < profileImages.Count)
            playerProfileImage.sprite = profileImages[imageIndex];
    }

    public void PauseGame()
    {
        GameManager.Instance.PauseGame();
        cameraController.enabled = false;
    }

    public void ResumeGame()
    {
        GameManager.Instance.ResumeGame();
        cameraController.enabled = true;
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
    }

    public void ShowGameOver(bool won)
    {
        if (winPanel == null || losePanel == null)
        {
            Debug.LogWarning("[UIManager] GameOver panels not assigned!");
            return;
        }

        winPanel.SetActive(won);
        losePanel.SetActive(!won);
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void ContinueButton()
    {
        // Hide both panels just in case
        if (winPanel != null)
            winPanel.SetActive(false);
        if (losePanel != null)
            losePanel.SetActive(false);

        // Resume the game
        GameManager.Instance.ResumeGame();

        cameraController.ResetCameraMovement();
    }

    public void RestartButton()
    {
        GameManager.Instance.RestartGame();
    }

    public void ExitToMainMenu()
    {
        GameManager.Instance.BackToMainMenu();
    }
}