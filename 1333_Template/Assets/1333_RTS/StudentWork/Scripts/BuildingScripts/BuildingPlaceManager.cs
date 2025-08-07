using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles live building placement and “move existing” edit mode.
public class BuildingPlaceManager : MonoBehaviour
{
    [SerializeField] private BuildingPlacer _placer;
    [SerializeField] private BuildingEditLogic _editLogic;

    private void Update()
    {
        // If editing but not in “move” sub-mode, ignore mouse.
        if (_editLogic.InEditMode && !_editLogic.CanMoveGhost)
            return;

        if (!_placer.IsPlacing || _placer.Ghost == null) return;

        // Snap ghost to grid under mouse
        Vector3 mousePos = _placer.GridManager.ClampWorldToGrid(GetMouseWorldPointOnGround());
        GridNode centerNode = _placer.GridManager.GetNodeFromWorldPosition(mousePos);
        if (centerNode == null) return;

        Vector3 offset = GetFootprintOffset(_placer.Footprint);
        Vector3 basePos = centerNode.WorldPosition - offset;
        bool canPlace = _placer.IsValidPlacementArea(basePos);

        _placer.Ghost.transform.position = centerNode.WorldPosition;
        BuildingGhostVisualizer.SetGhostColor(_placer.Ghost, canPlace ? Color.green : Color.red, 0.5f);

        if (Input.GetKeyDown(KeyCode.R))
        {
            _placer.RotateGhost(90f); // Use Y-axis rotation
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (_editLogic.InEditMode)
                _editLogic.CancelEdit();
            else
                _placer.CancelPlacement();
        }

        if (Input.GetMouseButtonDown(0) && canPlace)
        {

            if (_editLogic.InEditMode && _editLogic.CanMoveGhost)
            {
                GameObject real = _editLogic.CurrentBuilding;
                if (real != null)
                {

                    Vector3 oldBasePos = real.transform.position - GetFootprintOffset(_placer.Footprint);
                    for (int dx = 0; dx < _placer.Footprint.x; dx++)
                    {
                        for (int dy = 0; dy < _placer.Footprint.y; dy++)
                        {
                            Vector3 pos = oldBasePos + new Vector3(dx, 0, dy);
                            GridNode node = _placer.GridManager.GetNodeFromWorldPosition(pos);
                            if (node != null)
                                node.IsOccupied = false;
                        }
                    }

                    _placer.ApplyPlacement(real, centerNode, _placer.Footprint, _placer.CurrentRotation);
                    _editLogic.SetNewlyOccupiedNodes(centerNode, _placer.Footprint);
                    real.SetActive(true);

                    if (_placer.Ghost != null)
                    {
                        GameObject ghostToRemove = _placer.Ghost;
                        _placer.ClearGhostOnly();
                        Destroy(ghostToRemove);
                    }

                    //Debug.Log("[EditMode] Moved building using ApplyPlacement.");
                    _editLogic.DisableMoveMode();
                }
            }
            else
            {
                _placer.PlaceAtNode(centerNode);
            }
        }

    }

    // Helpers

    private Vector3 GetFootprintOffset(Vector2Int size)
    {
        return new Vector3((size.x - 1) / 2f, 0f, (size.y - 1) / 2f);
    }

    private Vector3 GetMouseWorldPointOnGround()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane ground = new(Vector3.up, Vector3.zero);
        return ground.Raycast(ray, out float enter) ? ray.GetPoint(enter) : Vector3.zero;
    }
}
