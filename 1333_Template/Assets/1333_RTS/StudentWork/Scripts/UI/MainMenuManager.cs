using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject slotPanel;
    public GameObject confirmationPanel;
    public GameObject gameName;
    public TMP_Text confirmationText;

    [Header("Load Options")]
    public GameObject loadOptionsPanel;
    public TMP_Text loadSlotNameText;

    [Header("Name Entry")]
    public GameObject nameEntryPanel;
    public TMP_InputField nameInputField;
    public Button startGameButton;
    public TMP_Text warningText;

    [Header("Slots")]
    public Button[] slotButtons;
    public TMP_Text[] slotTexts;

    private int selectedSlot = -1;
    private int selectedLoadSlot = -1;
    private Action confirmAction;

    private enum MenuMode { None, Start, Load }
    private MenuMode currentMode = MenuMode.None;

    // Entry points
    public void OpenStartSlots()
    {
        currentMode = MenuMode.Start;
        mainMenuPanel.SetActive(false);
        slotPanel.SetActive(true);
        gameName.SetActive(false);
        UpdateSlotDisplay();
    }

    public void OpenLoadSlots()
    {
        currentMode = MenuMode.Load;
        mainMenuPanel.SetActive(false);
        slotPanel.SetActive(true);
        gameName.SetActive(false);
        UpdateSlotDisplay();
    }

    public void BackToMain()
    {
        slotPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        confirmationPanel.SetActive(false);
        loadOptionsPanel.SetActive(false);
        nameEntryPanel.SetActive(false);
        gameName.SetActive(true);
    }

    // Slot rendering
    private void UpdateSlotDisplay()
    {
        for (int i = 0; i < slotTexts.Length; i++)
        {
            int slot = i + 1;
            string name = SaveManager.GetSlotName(slot);
            int days = SaveManager.GetLastPlayed(slot);

            if (string.IsNullOrEmpty(name))
            {
                slotTexts[i].text = "Empty Slot";
            }
            else
            {
                string timeInfo = days == 0 ? "Today"
                                : days == 1 ? "Yesterday"
                                : $"{days} days ago";
                slotTexts[i].text = $"{name} - Last played {timeInfo}";
            }

            int captured = slot;
            slotButtons[i].onClick.RemoveAllListeners();
            slotButtons[i].onClick.AddListener(() => OnSlotClicked(captured));
        }
    }

    // Unified slot handler
    public void OnSlotClicked(int slot)
    {
        selectedSlot = slot;

        if (currentMode == MenuMode.Start)
        {
            if (SaveManager.SlotExists(slot))
            {
                ShowConfirmationPanel("This slot already has data. Overwrite?", () =>
                {
                    OpenNameEntry(slot);
                });
            }
            else
            {
                OpenNameEntry(slot);
            }
        }
        else if (currentMode == MenuMode.Load)
        {
            if (SaveManager.SlotExists(slot))
            {
                ShowLoadOptionsPanel(slot);
            }
        }
    }

    public void OpenNameEntry(int slot)
    {
        selectedSlot = slot;
        nameInputField.text = "";
        startGameButton.interactable = false;
        nameEntryPanel.SetActive(true);
        warningText.text = "";
    }

    public void OnNameInputChanged()
    {
        string name = nameInputField.text.Trim();
        if (name.Length < 3)
        {
            startGameButton.interactable = false;
            warningText.text = "Name must be at least 3 characters";
        }
        else
        {
            startGameButton.interactable = true;
            warningText.text = "";
        }
    }

    public void ConfirmStart()
    {
        string playerName = nameInputField.text.Trim();
        if (string.IsNullOrEmpty(playerName)) return;

        SaveManager.SaveSlot(selectedSlot, playerName);
        SceneManager.LoadScene("PlayerScene");
    }

    public void CancelNameEntry()
    {
        nameEntryPanel.SetActive(false);
        UpdateSlotDisplay();
    }

    // Load Option Panel
    public void ShowLoadOptionsPanel(int slot)
    {
        selectedLoadSlot = slot;
        string name = SaveManager.GetSlotName(slot);
        int days = SaveManager.GetLastPlayed(slot);
        string info = days == 0 ? "Today" : days == 1 ? "Yesterday" : $"{days} days ago";

        loadSlotNameText.text = $"{name} - Last played {info}";
        loadOptionsPanel.SetActive(true);
    }

    public void OnClickLoadStart()
    {
        string name = SaveManager.GetSlotName(selectedLoadSlot);
        if (!string.IsNullOrEmpty(name))
        {
            SaveManager.SaveSlot(selectedLoadSlot, name);
            SceneManager.LoadScene("PlayerScene");
        }
    }

    public void OnClickLoadDelete()
    {
        ShowConfirmationPanel("Are you sure you want to delete this save?", () =>
        {
            SaveManager.DeleteSlot(selectedLoadSlot);
            loadOptionsPanel.SetActive(false);
            UpdateSlotDisplay();
        });
    }

    public void OnClickLoadCancel()
    {
        loadOptionsPanel.SetActive(false);
    }

    // Confirmation Panel
    public void ShowConfirmationPanel(string message, Action onConfirm)
    {
        confirmationText.text = message;
        confirmAction = onConfirm;
        confirmationPanel.SetActive(true);
    }

    public void OnConfirmYes()
    {
        confirmAction?.Invoke();
        confirmationPanel.SetActive(false);
    }

    public void OnConfirmNo()
    {
        confirmAction = null;
        confirmationPanel.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
