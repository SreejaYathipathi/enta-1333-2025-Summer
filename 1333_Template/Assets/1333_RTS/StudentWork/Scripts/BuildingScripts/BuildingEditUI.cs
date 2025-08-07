using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// UI panel that appears when the player selects a building to edit.
/// Buttons forward actions to BuildingEditLogic.
/// </summary>
public class BuildingEditUI : MonoBehaviour
{
    public static BuildingEditUI Instance;

    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Button _rotateButton;
    [SerializeField] private Button _moveButton;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private BuildingEditLogic _editLogic;
    [SerializeField] private BuildingPlacer _placer;


    private GameObject selectedBuilding;

    private void Awake()
    {
        Instance = this;
        _panel.SetActive(false);

        // Hook up button events
        _cancelButton.onClick.AddListener(OnCancel);
        _rotateButton.onClick.AddListener(OnRotate);
        _moveButton.onClick.AddListener(OnMove);
        _confirmButton.onClick.AddListener(OnConfirm);
    }

    public void ShowUI(GameObject building)
    {
        selectedBuilding = building;
        _panel.SetActive(true);

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
            _placer.RotateGhost(90f);
        }
    }

    private void OnMove()
    {
        _editLogic.EnableMoveMode();
        Debug.Log("Move mode enabled — click to place building.");
    }

    private void OnConfirm()
    {
        _editLogic.ConfirmGhostPlacement();
        HideUI();
    }
}
