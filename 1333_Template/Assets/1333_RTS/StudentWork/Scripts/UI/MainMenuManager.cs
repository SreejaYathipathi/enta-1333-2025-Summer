using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class SlotUI
{
    public Image avatarImage;
    public TMP_Text nameText;
    public TMP_Text createdText;
    public TMP_Text openedText;
}

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
    public SlotUI[] slots;

    [Header("Profile Images")]
    public Image selectedImageDisplay;
    public List<Sprite> profileImages;
    public int selectedImageIndex = -1;

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

    private void UpdateSlotDisplay()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            int slot = i + 1;

            if (!SaveManager.SlotExists(slot))
            {
                slots[i].nameText.text = "Empty Slot";
                slots[i].createdText.text = "";
                slots[i].openedText.text = "";
                if (slots[i].avatarImage != null)
                    slots[i].avatarImage.sprite = null;
            }
            else
            {
                string name = SaveManager.GetSlotName(slot);
                int createdDays = SaveManager.GetCreatedDaysAgo(slot);
                int openedDays = SaveManager.GetLastPlayed(slot);
                int avatarIndex = SaveManager.GetProfileImageIndex(slot);

                slots[i].nameText.text = name;
                slots[i].createdText.text = createdDays >= 0 ? $"Created {createdDays} days ago" : "";
                slots[i].openedText.text = openedDays >= 0 ? $"Opened {openedDays} days ago" : "";

                if (slots[i].avatarImage != null && avatarIndex >= 0 && avatarIndex < profileImages.Count)
                    slots[i].avatarImage.sprite = profileImages[avatarIndex];
            }

            int capturedSlot = slot;
            slotButtons[i].onClick.RemoveAllListeners();
            slotButtons[i].onClick.AddListener(() => OnSlotClicked(capturedSlot));
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
                nameEntryPanel.SetActive(false);

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

    public void OnProfileImageSelected(int index)
    {
        if (index < 0 || index >= profileImages.Count)
            return;

        selectedImageIndex = index;
        selectedImageDisplay.sprite = profileImages[index]; // Show in white circle
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

        // If no image selected, fallback to default (index 0)
        int imageIndex = selectedImageIndex >= 0 ? selectedImageIndex : 0;

        SaveManager.SaveSlot(selectedSlot, playerName, imageIndex);
        GameManager.Instance.StartGame();
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

        loadSlotNameText.text = $"{name} - Last played {days} days agao";
        loadOptionsPanel.SetActive(true);
    }

    public void OnClickLoadStart()
    {
        string name = SaveManager.GetSlotName(selectedLoadSlot);
        int imageIndex = SaveManager.GetProfileImageIndex(selectedLoadSlot); // NEW

        if (!string.IsNullOrEmpty(name) && imageIndex >= 0)
        {
            SaveManager.SaveSlot(selectedLoadSlot, name, imageIndex);
            GameManager.Instance.StartGame();
        }
        else
        {
            Debug.LogWarning("Slot missing name or image index!");
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
        loadOptionsPanel.SetActive(false);
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
