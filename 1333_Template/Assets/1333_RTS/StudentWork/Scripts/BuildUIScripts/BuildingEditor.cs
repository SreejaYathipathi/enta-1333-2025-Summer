using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingEditor : MonoBehaviour
{
    [SerializeField] private BuildingPlacer _placer;
    [SerializeField] private BuildingEditable _editable;

    private void Update()
    {
        if (!_placer.IsPlacing || _placer.Ghost == null) return;

        Vector3 mousePos = _placer.GridManager.ClampWorldToGrid(GetMouseWorldPointOnGround());
        GridNode centerNode = _placer.GridManager.GetNodeFromWorldPosition(mousePos);
        if (centerNode == null) return;

        Vector3 offset = GetFootprintOffset(_placer.Footprint);
        Vector3 basePos = centerNode.WorldPosition - offset;
        bool canPlace = _placer.IsValidPlacementArea(basePos);

        _placer.Ghost.transform.position = centerNode.WorldPosition;
        BuildGhostVisualizer.SetGhostColor(_placer.Ghost, canPlace ? Color.green : Color.red, 0.5f);

        if (Input.GetKeyDown(KeyCode.R))
            _placer.Ghost.transform.Rotate(Vector3.up * 90f);

        if (Input.GetMouseButtonDown(1))
        {
            if (_editable.InEditMode)
                _editable.CancelEdit();
            else
                _placer.CancelPlacement();
        }

        if (Input.GetMouseButtonDown(0) && canPlace)
        {
            if (_placer.IsEditPlacement)
            {
                Debug.Log("You're editing a building.");
                _editable.ConfirmEdit(centerNode.WorldPosition, _placer.Ghost.transform.rotation);
            }
            else
            {
                Debug.Log("You're placing a new building.");
                _placer.PlaceAtNode(centerNode);
                _editable.NotifyNewPlacement();
            }
        }

        if (!_placer.IsPlacing && IsBuildUIOpen())
        {
            _editable.TrySelectBuilding();
        }
    }

    private bool IsBuildUIOpen()
    {
        return GameObject.FindObjectOfType<BuildUiManager>().bottomPanel.activeSelf;
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
