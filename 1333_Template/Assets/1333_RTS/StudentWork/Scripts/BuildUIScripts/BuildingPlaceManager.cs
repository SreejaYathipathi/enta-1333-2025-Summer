using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPlaceManager : MonoBehaviour
{
    [SerializeField] private BuildingPlacer _placer;
    [SerializeField] private BuildingEditLogic _editLogic;

    private void Update()
    {
        if (_editLogic.InEditMode && !_editLogic.CanMoveGhost)
            return;

        if (!_placer.IsPlacing || _placer.Ghost == null) return;

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
                Vector3 newPos = centerNode.WorldPosition;

                GameObject real = _editLogic.CurrentBuilding;
                if (real != null)
                {
                    float liftAmount = _placer.GridManager.GridSettings.NodeSize + 0.1f;
                    Vector3 liftedPos = newPos + Vector3.up * liftAmount;

                    real.transform.position = liftedPos;
                    real.SetActive(true);
                }

                if (_placer.Ghost != null)
                {
                    GameObject ghostToRemove = _placer.Ghost;
                    _placer.ClearGhostOnly();
                    Destroy(ghostToRemove);
                }

                Debug.Log("[EditMode] Real building moved to new location (preview)");
                _editLogic.DisableMoveMode();
            }
            else
            {
                _placer.PlaceAtNode(centerNode);
            }
        }

    }

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
