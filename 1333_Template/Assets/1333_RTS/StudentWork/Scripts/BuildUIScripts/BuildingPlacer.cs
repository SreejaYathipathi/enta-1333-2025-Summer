using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class BuildingPlacer : MonoBehaviour
{

    [SerializeField] private GridManager _gridManager;

    private GameObject _ghostBuilding;
    private BuildingItemData _currentBuildData;
    private Vector2Int _footprint;
    private float _currentRotation = 0f;
    public float CurrentRotation => _currentRotation;

    private bool _isEditPlacement = false;
    public bool IsEditPlacement => _isEditPlacement;

    private bool _isPlacing;

    public void SetPrefabToPlace(BuildingItemData data)
    {
        if (_ghostBuilding != null)
            Destroy(_ghostBuilding);

        _currentBuildData = data;
        _footprint = data.footprintSize;
        _ghostBuilding = Instantiate(data.prefab);
        BuildingGhostVisualizer.MakeGhost(_ghostBuilding);

        _isPlacing = true;
        _isEditPlacement = false;
        _currentRotation = 0f;
    }

    public void ApplyPlacement(GameObject target, GridNode centerNode, Vector2Int footprint, float rotationY)
    {
        Vector3 offset = GetFootprintOffset(footprint);
        Vector3 basePos = centerNode.WorldPosition - offset;

        if (!IsValidPlacementArea(basePos)) return;

        float nodeHeight = _gridManager.GridSettings.NodeSize;
        Vector3 liftedPosition = centerNode.WorldPosition + Vector3.up * (nodeHeight + 0.1f);

        target.transform.position = liftedPosition;
        target.transform.rotation = Quaternion.Euler(-90f, rotationY, 0f);

        foreach (var col in target.GetComponentsInChildren<Collider>())
            col.enabled = true;

        foreach (var script in target.GetComponents<MonoBehaviour>())
            script.enabled = true;

        BuildingGhostVisualizer.MakeReal(target);

        for (int dx = 0; dx < footprint.x; dx++)
        {
            for (int dy = 0; dy < footprint.y; dy++)
            {
                //Vector3 pos = basePos + new Vector3(dx, 0, dy);

                Vector3 localOffset = RotateOffset(new Vector3(dx, 0, dy), rotationY);
                Vector3 pos = centerNode.WorldPosition - GetFootprintOffsetRotated(footprint, rotationY) + localOffset;

                GridNode node = _gridManager.GetNodeFromWorldPosition(pos);
                if (node != null)
                    node.IsOccupied = true;
            }
        }
    }

    public void PlaceAtNode(GridNode centerNode)
    {
        if (_ghostBuilding == null || _currentBuildData == null) return;

        ApplyPlacement(Instantiate(_ghostBuilding), centerNode, _footprint, _currentRotation);

        Destroy(_ghostBuilding);
        _ghostBuilding = null;
        _isPlacing = false;
    }

    private Vector3 RotateOffset(Vector3 offset, float angleY)
    {
        Quaternion rotation = Quaternion.Euler(0, angleY, 0);
        return rotation * offset;
    }

    private Vector3 GetFootprintOffsetRotated(Vector2Int footprint, float angleY)
    {
        Vector3 centerOffset = new Vector3((footprint.x - 1) / 2f, 0f, (footprint.y - 1) / 2f);
        return RotateOffset(centerOffset, angleY);
    }

    public bool IsValidPlacementArea(Vector3 basePos)
    {
        for (int dx = 0; dx < _footprint.x; dx++)
        {
            for (int dy = 0; dy < _footprint.y; dy++)
            {
                Vector3 pos = basePos + new Vector3(dx, 0, dy);
                GridNode node = _gridManager.GetNodeFromWorldPosition(pos);
                if (node == null || !node.Walkable || node.IsOccupied)
                    return false;
            }
        }
        return true;
    }

    public void CancelPlacement()
    {
        if (_ghostBuilding != null)
            Destroy(_ghostBuilding);

        _ghostBuilding = null;
        _isPlacing = false;
        _isEditPlacement = false;
    }

    public bool IsPlacing => _isPlacing;
    public GameObject Ghost => _ghostBuilding;
    public Vector2Int Footprint => _footprint;
    public GridManager GridManager => _gridManager;

    private Vector3 GetFootprintOffset(Vector2Int size)
    {
        return new Vector3((size.x - 1) / 2f, 0f, (size.y - 1) / 2f);
    }

    public void SetExistingGhost(GameObject ghost)
    {
        if (_ghostBuilding != null)
            Destroy(_ghostBuilding);

        _ghostBuilding = ghost;
        _isPlacing = true;
        _isEditPlacement = true;
    }

    public void RotateGhost(float angle)
    {
        _currentRotation += angle;
        _ghostBuilding.transform.rotation = Quaternion.Euler(-90f, _currentRotation, 0f);
    }

    public void ClearGhostOnly()
    {
        if (_ghostBuilding != null)
            Destroy(_ghostBuilding);

        _ghostBuilding = null;
        _isPlacing = false;
        _isEditPlacement = false;
    }
}
