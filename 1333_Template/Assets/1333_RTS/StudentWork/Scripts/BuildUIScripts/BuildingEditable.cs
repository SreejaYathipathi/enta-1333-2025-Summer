using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingEditable : MonoBehaviour
{
    [SerializeField] private BuildingPlacer _placer;
    [SerializeField] private LayerMask _buildingLayer;

    private GameObject _originalBuilding;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;

    private float _editClickCooldown = 0.2f;
    private float _lastPlacementTime = -1f;

    private bool _inEditMode = false;

    public bool InEditMode => _inEditMode;

    public void EnterEditMode()
    {
        _inEditMode = true;
    }

    public void ExitEditMode()
    {
        _inEditMode = false;
    }

    public void NotifyNewPlacement()
    {
        _lastPlacementTime = Time.time;
    }

    public void TrySelectBuilding()
    {
        if (_placer.IsPlacing) return;

        // Skip if we just placed something
        if (Time.time - _lastPlacementTime < _editClickCooldown)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, _buildingLayer))
            {
                GameObject building = hit.collider.transform.root.gameObject;

                if (building.layer != LayerMask.NameToLayer("Building"))
                    return;

                _inEditMode = true;

                _originalBuilding = building;
                _originalPosition = building.transform.position;
                _originalRotation = building.transform.rotation;

                GameObject ghost = Instantiate(building);
                BuildGhostVisualizer.MakeGhost(ghost);
                ghost.transform.position = _originalPosition;
                ghost.transform.rotation = _originalRotation;

                building.SetActive(false);
                _placer.SetExistingGhost(ghost);
            }
        }
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
    }

    public void ConfirmEdit(Vector3 newPosition, Quaternion newRotation)
    {
        if (_originalBuilding != null)
        {
            _originalBuilding.transform.position = newPosition;
            _originalBuilding.transform.rotation = newRotation;
            _originalBuilding.SetActive(true);
        }

        _placer.ClearGhostOnly();
        _originalBuilding = null;
        _inEditMode = false;
    }
}
