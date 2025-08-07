using System.Collections;
using System.Collections.Generic;
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

    public static BuildingPlacer Instance { get; private set; }

    public List<GameObject> placedBuildings = new List<GameObject>();

    public bool IsPlacing => _isPlacing;
    public GameObject Ghost => _ghostBuilding;
    public Vector2Int Footprint => _footprint;
    public GridManager GridManager => _gridManager;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SetPrefabToPlace(BuildingItemData data)
    {

        if (!ResourceManager.Instance.HasResources(data.costs))
        {
            Debug.Log("Not enough resources.");
            return;
        }

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

        Bounds bounds = GetRendererBounds(target);

        float bottomOffset = bounds.center.y - bounds.extents.y;
        Vector3 correctedPos = centerNode.WorldPosition - new Vector3(0f, bottomOffset, 0f);
        target.transform.position = correctedPos;

        target.transform.rotation = Quaternion.Euler(-90f, rotationY, 0f);

        var bh = target.GetComponent<BuildingHealth>();
        if (bh != null)
        {
            if (_currentBuildData)
                bh.purpose = _currentBuildData.purpose;

            //bh.FootprintSize = _currentBuildData.footprintSize;

            bh.FootprintSize = footprint;
        }

        foreach (var col in target.GetComponentsInChildren<Collider>())
            col.enabled = true;

        foreach (var script in target.GetComponents<MonoBehaviour>())
            script.enabled = true;

        BuildingGhostVisualizer.MakeReal(target);

        for (int dx = 0; dx < footprint.x; dx++)
        {
            for (int dy = 0; dy < footprint.y; dy++)
            {
                Vector3 pos = basePos + new Vector3(dx, 0, dy);
                GridNode node = _gridManager.GetNodeFromWorldPosition(pos);
                if (node != null)
                    node.IsOccupied = true;
            }
        }

        if (_currentBuildData)
            XPManager.Instance.AddXP(_currentBuildData.xpReward);
    }

    public void PlaceAtNode(GridNode centerNode)
    {
        if (_ghostBuilding == null || _currentBuildData == null) return;

        ApplyPlacement(Instantiate(_ghostBuilding), centerNode, _footprint, _currentRotation);

        ResourceManager.Instance.SpendResources(_currentBuildData.costs);

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

    private Bounds GetRendererBounds(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(go.transform.position, Vector3.zero);

        Bounds combined = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            combined.Encapsulate(renderers[i].bounds);
        }
        return combined;
    }

    public void CancelPlacement()
    {
        if (_ghostBuilding != null)
            Destroy(_ghostBuilding);

        _ghostBuilding = null;
        _isPlacing = false;
        _isEditPlacement = false;
    }

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

    private void RegisterBuilding(GameObject building)
    {
        placedBuildings.Add(building);
    }

    public void PlaceBuildingFromSave(GameObject prefab, Vector3 worldPos, float rotationY)
    {
        Vector3 snappedPos = SnapToGrid(worldPos);

        GameObject newBuilding = Instantiate(
            prefab,
            snappedPos,
            Quaternion.Euler(-90f, rotationY, 0f));

        BuildingItemData itemData = GameManager.Instance
                                               .prefabDatabase
                                               .GetItemDataByPrefabName(prefab.name);

        Vector2Int footprint = itemData ? itemData.footprintSize : Vector2Int.one;
        _currentBuildData = itemData;

        Vector3 basePos = snappedPos - GetFootprintOffset(footprint);
        for (int dx = 0; dx < footprint.x; dx++)
            for (int dy = 0; dy < footprint.y; dy++)
            {
                Vector3 pos = basePos + new Vector3(dx, 0, dy);
                GridNode node = _gridManager.GetNodeFromWorldPosition(pos);
                if (node != null) node.IsOccupied = true;
            }

        var bh = newBuilding.GetComponent<BuildingHealth>();
        if (bh != null)
        {
            if (itemData) bh.purpose = itemData.purpose;
            bh.FootprintSize = footprint;
        }

        RegisterBuilding(newBuilding);
    }

    private Vector3 SnapToGrid(Vector3 position)
    {
        //float gridSize = 1f; // your grid cell size

        float gridSize = _gridManager.GridSettings.NodeSize;
        return new Vector3(
            Mathf.Round(position.x / gridSize) * gridSize,
            position.y,
            Mathf.Round(position.z / gridSize) * gridSize
        );
    }

}
