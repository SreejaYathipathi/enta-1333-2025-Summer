using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class BuildingPlacer : MonoBehaviour
{

    [SerializeField] private GridManager _gridManager;

    private GameObject _ghostBuilding;
    private BuildItemData _currentBuildData;
    private Vector2Int _footprint;
    private float _currentRotation = 0f;

    private bool _isEditPlacement = false;
    public bool IsEditPlacement => _isEditPlacement;

    private bool _isPlacing;

    public void SetPrefabToPlace(BuildItemData data)
    {
        if (_ghostBuilding != null)
            Destroy(_ghostBuilding);

        _currentBuildData = data;
        _footprint = data.footprintSize;
        _ghostBuilding = Instantiate(data.prefab);
        BuildGhostVisualizer.MakeGhost(_ghostBuilding);

        _isPlacing = true;
        _isEditPlacement = false;
        _currentRotation = 0f;
    }

    public void PlaceAtNode(GridNode centerNode)
    {
        Vector3 offset = GetFootprintOffset(_footprint);
        Vector3 basePos = centerNode.WorldPosition - offset;

        if (!IsValidPlacementArea(basePos)) return;

        GameObject placed = Instantiate(_ghostBuilding);
        //placed.transform.position = centerNode.WorldPosition;

        float nodeHeight = _gridManager.GridSettings.NodeSize;
        Vector3 liftedPosition = centerNode.WorldPosition + Vector3.up * (nodeHeight + 0.1f); // Raise more than units
        placed.transform.position = liftedPosition;

        placed.transform.rotation = _ghostBuilding.transform.rotation;

        foreach (var col in placed.GetComponentsInChildren<Collider>())
            col.enabled = true;

        foreach (var script in placed.GetComponents<MonoBehaviour>())
            script.enabled = true;

        BuildGhostVisualizer.MakeReal(placed);

        for (int dx = 0; dx < _footprint.x; dx++)
        {
            for (int dy = 0; dy < _footprint.y; dy++)
            {
                Vector3 pos = basePos + new Vector3(dx, 0, dy);
                GridNode node = _gridManager.GetNodeFromWorldPosition(pos);
                if (node != null)
                    node.IsOccupied = true;
            }
        }

        Destroy(_ghostBuilding);
        _ghostBuilding = null;
        _isPlacing = false;
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

    public void ClearGhostOnly()
    {
        if (_ghostBuilding != null)
            Destroy(_ghostBuilding);

        _ghostBuilding = null;
        _isPlacing = false;
        _isEditPlacement = false;
    }
}
