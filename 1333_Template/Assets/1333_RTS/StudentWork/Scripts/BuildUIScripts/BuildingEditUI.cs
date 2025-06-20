using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingEditUI : MonoBehaviour
{
    public static BuildingEditUI Instance;

    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Button _rotateButton;
    [SerializeField] private Button _moveButton;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private BuildingEditLogic _editLogic;

    private GameObject selectedBuilding;

    private void Awake()
    {
        Instance = this;
        _panel.SetActive(false);

        _cancelButton.onClick.AddListener(OnCancel);
        _rotateButton.onClick.AddListener(OnRotate);
        _moveButton.onClick.AddListener(OnMove);
        _confirmButton.onClick.AddListener(OnConfirm);
    }

    public void ShowUI(GameObject building)
    {
        selectedBuilding = building;
        _panel.SetActive(true);

        Vector3 screenPos = Camera.main.WorldToScreenPoint(building.transform.position);
        _panel.transform.position = screenPos + new Vector3(0, 80, 0); // offset

        // Set up ghost
        _editLogic.EnterEditModeFromBuilding(building);
    }

    public void HideUI()
    {
        _panel.SetActive(false);
        selectedBuilding = null;
    }

    private void OnCancel()
    {
        _editLogic.CancelEdit();
        HideUI();
    }

    private void OnRotate()
    {
        if (_editLogic.InEditMode)
        {
            _editLogic.RotateGhost(90f);
        }
    }

    private void OnMove()
    {
        // Stay in edit mode – user can reposition the ghost
        // UI stays visible
    }

    private void OnConfirm()
    {
        _editLogic.ConfirmGhostPlacement();
        HideUI();
    }
}
