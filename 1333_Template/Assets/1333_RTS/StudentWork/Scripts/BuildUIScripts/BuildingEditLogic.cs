using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingEditLogic : MonoBehaviour
{
    [SerializeField] private BuildingPlacer _placer;
    [SerializeField] private LayerMask _buildingLayer;

    private GameObject _originalBuilding;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;

    private bool _inEditMode = false;
    private bool _isMoveModeActive = false;

    public bool InEditMode => _inEditMode;
    public bool CanMoveGhost => _inEditMode && _isMoveModeActive;

    public void EnterEditModeFromBuilding(GameObject building)
    {
        _originalBuilding = building;
        _originalPosition = building.transform.position;
        _originalRotation = building.transform.rotation;

        GameObject ghost = Instantiate(building);
        BuildingGhostVisualizer.MakeGhost(ghost);
        ghost.transform.position = _originalPosition;
        ghost.transform.rotation = _originalRotation;

        building.SetActive(false);
        _placer.SetExistingGhost(ghost);

        _inEditMode = true;
        _isMoveModeActive = false;
    }

    public void EnableMoveMode()
    {
        if (_inEditMode)
            _isMoveModeActive = true;
    }

    public void ConfirmGhostPlacement()
    {
        if (_originalBuilding != null)
        {
            _originalBuilding.transform.position = _placer.Ghost.transform.position;
            _originalBuilding.transform.rotation = _placer.Ghost.transform.rotation;
            _originalBuilding.SetActive(true);
        }

        _placer.ClearGhostOnly();
        _originalBuilding = null;
        _inEditMode = false;
        _isMoveModeActive = false;
    }

    public void CancelEdit()
    {
        if (_originalBuilding != null)
        {
            _originalBuilding.transform.position = _originalPosition;
            _originalBuilding.transform.rotation = _originalRotation;
            _originalBuilding.SetActive(true);
        }

        _placer.CancelPlacement();
        _originalBuilding = null;
        _inEditMode = false;
        _isMoveModeActive = false;
    }
}
