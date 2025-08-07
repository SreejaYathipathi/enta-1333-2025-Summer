using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles “edit existing building” flow:
/// Convert the selected building into a movable ghost
/// Let the player reposition / rotate it
/// Confirm or cancel changes, updating GridNodes’ occupied flags
/// </summary>
public class BuildingEditLogic : MonoBehaviour
{
    [SerializeField] private BuildingPlacer _placer;
    [SerializeField] private LayerMask _buildingLayer;

    private List<GridNode> _newlyOccupiedNodes = new();

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

    /// <summary>
    /// Called when player taps an existing building to edit.
    /// </summary>
    public void EnterEditModeFromBuilding(GameObject building)
    {
        // Cache original transform so we can restore on cancel
        _originalBuilding = building;
        _originalPosition = building.transform.position;
        _originalRotation = building.transform.rotation;

        // Spawn a ghost clone the player can move
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

    /// <summary>
    /// Player clicks “confirm” – apply ghost transform to real object.
    /// </summary>
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

                //Vector3 oldBasePos = oldPos - GetFootprintOffset(footprint);

                Vector3 oldBasePos = new Vector3(Mathf.Round(_originalPosition.x / _placer.GridManager.GridSettings.NodeSize) * _placer.GridManager.GridSettings.NodeSize, 0,
                Mathf.Round(_originalPosition.z / _placer.GridManager.GridSettings.NodeSize) * _placer.GridManager.GridSettings.NodeSize) - GetFootprintOffset(footprint);

            }

            _originalBuilding = null;
        }

        _placer.ClearGhostOnly();
        _originalBuilding = null;
        _inEditMode = false;
        _isMoveModeActive = false;
    }

    /// <summary>
    /// Player clicks “cancel” – restore original transform & grid flags.
    /// </summary>
    public void CancelEdit()
    {
        if (_originalBuilding != null)
        {
            _originalBuilding.transform.position = _originalPosition;
            _originalBuilding.transform.rotation = _originalRotation;
            _originalBuilding.SetActive(true);


            Vector3 basePos = _originalPosition - GetFootprintOffset(_placer.Footprint);
            for (int dx = 0; dx < _placer.Footprint.x; dx++)
            {
                for (int dy = 0; dy < _placer.Footprint.y; dy++)
                {
                    Vector3 pos = basePos + new Vector3(dx, 0, dy);
                    GridNode node = _placer.GridManager.GetNodeFromWorldPosition(pos);
                    if (node != null)
                        node.IsOccupied = true;
                }
            }

            foreach (var node in _newlyOccupiedNodes)
            {
                node.IsOccupied = false;
            }
        }

        _placer.ClearGhostOnly();

        _placer.CancelPlacement();
        _originalBuilding = null;
        _inEditMode = false;
        _isMoveModeActive = false;
    }

    /// <summary>
    /// Called by BuildingPlacer each time the ghost moves to update temp node list.
    /// </summary>
    public void SetNewlyOccupiedNodes(GridNode centerNode, Vector2Int footprint)
    {
        _newlyOccupiedNodes.Clear();
        Vector3 basePos = centerNode.WorldPosition - GetFootprintOffset(footprint);

        for (int dx = 0; dx < footprint.x; dx++)
        {
            for (int dy = 0; dy < footprint.y; dy++)
            {
                Vector3 pos = basePos + new Vector3(dx, 0, dy);
                GridNode node = _placer.GridManager.GetNodeFromWorldPosition(pos);
                if (node != null)
                    _newlyOccupiedNodes.Add(node);
            }
        }
    }

    // Converts footprint size (width, height) to offset so center aligns to nodes
    private Vector3 GetFootprintOffset(Vector2Int size)
    {
        return new Vector3((size.x - 1) / 2f, 0f, (size.y - 1) / 2f);
    }
}
