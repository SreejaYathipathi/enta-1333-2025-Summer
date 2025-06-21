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

    public GameObject CurrentBuilding => _originalBuilding;

    public void DisableMoveMode()
    {
        _isMoveModeActive = false;
    }

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
            // Only apply position/rotation from ghost if it exists
            if (_placer.Ghost != null)
            {
                Vector3 oldPos = _originalPosition;
                Vector3 newPos = _placer.Ghost.transform.position;
                Vector2Int footprint = _placer.Footprint;

                _originalBuilding.transform.position = newPos;
                _originalBuilding.transform.rotation = _placer.Ghost.transform.rotation;
                _originalBuilding.SetActive(true);

                // Update the grid
                UpdateGridOccupation(oldPos, newPos, footprint);

            }

            _originalBuilding = null;
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

        _placer.ClearGhostOnly();

        _placer.CancelPlacement();
        _originalBuilding = null;
        _inEditMode = false;
        _isMoveModeActive = false;
    }

    private void UpdateGridOccupation(Vector3 oldPos, Vector3 newPos, Vector2Int footprint)
    {
        ClearGridOccupation(oldPos, footprint);
        SetGridOccupation(newPos, footprint);
    }

    private void ClearGridOccupation(Vector3 centerPos, Vector2Int footprint)
    {
        Vector3 offset = GetFootprintOffset(footprint);
        Vector3 basePos = centerPos - offset;

        for (int x = 0; x < footprint.x; x++)
        {
            for (int y = 0; y < footprint.y; y++)
            {
                Vector3 pos = basePos + new Vector3(x, 0, y);
                GridNode node = _placer.GridManager.GetNodeFromWorldPosition(pos);
                if (node != null)
                {
                    node.IsOccupied = false;
                }
            }
        }
    }

    private void SetGridOccupation(Vector3 centerPos, Vector2Int footprint)
    {
        Vector3 offset = GetFootprintOffset(footprint);
        Vector3 basePos = centerPos - offset;

        for (int x = 0; x < footprint.x; x++)
        {
            for (int y = 0; y < footprint.y; y++)
            {
                Vector3 pos = basePos + new Vector3(x, 0, y);
                GridNode node = _placer.GridManager.GetNodeFromWorldPosition(pos);
                if (node != null)
                {
                    node.IsOccupied = true;
                }
            }
        }
    }

    private Vector3 GetFootprintOffset(Vector2Int size)
    {
        return new Vector3((size.x - 1) / 2f, 0f, (size.y - 1) / 2f);
    }
}
