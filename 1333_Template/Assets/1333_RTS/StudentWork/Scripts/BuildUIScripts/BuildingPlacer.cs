using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class BuildingPlacer : MonoBehaviour
{

    [SerializeField] private GridManager _gridManager;

    private GameObject _ghostBuilding;
    private bool _isPlacing = false;
    private float _currentRotation = 0f;
    private Vector2Int _currentFootprint = Vector2Int.one;
    private BuildItemData _currentBuildData;

    private Material[] _originalMaterials;

    public void SetPrefabToPlace(BuildItemData data)
    {
        Debug.Log("[Placer] UI button clicked — preparing ghost preview");

        if (_ghostBuilding != null)
            Destroy(_ghostBuilding);

        _currentBuildData = data;
        _currentFootprint = data.footprintSize;

        _ghostBuilding = Instantiate(data.prefab);

        foreach (Renderer renderer in _ghostBuilding.GetComponentsInChildren<Renderer>())
        {
            renderer.materials = (Material[])renderer.materials.Clone();
        }

        _originalMaterials = GetAllMaterials(_ghostBuilding);
        SetAsGhost(_ghostBuilding);
        _isPlacing = true;
        _currentRotation = 0f;

    }

    private void Update()
    {
        if (!_isPlacing || _gridManager == null || _ghostBuilding == null) return;

        HandleGhostPositionAndColor();
        HandleRotationInput();
        HandleCancelInput();
        HandlePlacementInput();
    }

    private void HandleGhostPositionAndColor()
    {
        Vector3 mousePos = _gridManager.ClampWorldToGrid(GetMouseWorldPointOnGround());

        GridNode centerNode = _gridManager.GetNodeFromWorldPosition(mousePos);
        if (centerNode == null) return;

        Vector3 offset = GetFootprintOffset(_currentBuildData.footprintSize);
        Vector3 basePos = centerNode.WorldPosition - offset;

        bool canPlace = IsValidPlacementArea(basePos, _currentBuildData.footprintSize);
        SetGhostColor(canPlace ? Color.green : Color.red, 0.5f);

        _ghostBuilding.transform.position = centerNode.WorldPosition;
    }

    private void HandleRotationInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            _currentRotation += 90f;
            _currentRotation %= 360f;

            Vector3 originalEuler = _ghostBuilding.transform.rotation.eulerAngles;
            _ghostBuilding.transform.rotation = Quaternion.Euler(originalEuler.x, _currentRotation, originalEuler.z);

            Debug.Log("[Placer] Rotated ghost to Y=" + _currentRotation + " degrees");
        }
    }

    private void HandleCancelInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("[Placer] Placement cancelled (right click)");
            CancelPlacement();
        }
    }

    private void HandlePlacementInput()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector3 mousePos = GetMouseWorldPointOnGround();
        GridNode centerNode = _gridManager.GetNodeFromWorldPosition(mousePos);
        if (centerNode == null) return;

        Vector3 offset = GetFootprintOffset(_currentFootprint);
        Vector3 basePos = centerNode.WorldPosition - offset;

        if (!IsValidPlacementArea(basePos, _currentFootprint))
        {
            Debug.Log("[Placer] Cannot place — invalid or occupied node.");
            return;
        }

        GameObject placed = Instantiate(_ghostBuilding);
        placed.transform.position = centerNode.WorldPosition;
        placed.transform.rotation = _ghostBuilding.transform.rotation;

        FinalizePlacement(placed);

        // ✅ Mark nodes only ONCE here
        for (int dx = 0; dx < _currentFootprint.x; dx++)
        {
            for (int dy = 0; dy < _currentFootprint.y; dy++)
            {
                Vector3 checkPos = basePos + new Vector3(dx, 0, dy);
                GridNode occNode = _gridManager.GetNodeFromWorldPosition(checkPos);
                if (occNode != null)
                {
                    occNode.IsOccupied = true;
                    Debug.Log($"[Placer] Marked node at {occNode.WorldPosition} as occupied");
                }
            }
        }

        Debug.Log("[Placer] Final building spawned and finalized at: " + placed.transform.position);

        Destroy(_ghostBuilding);
        _ghostBuilding = null;
        _isPlacing = false;
    }

    private bool IsValidPlacementArea(Vector3 basePos, Vector2Int size)
    {
        for (int dx = 0; dx < size.x; dx++)
        {
            for (int dy = 0; dy < size.y; dy++)
            {
                Vector3 checkPos = basePos + new Vector3(dx, 0, dy);
                GridNode checkNode = _gridManager.GetNodeFromWorldPosition(checkPos);
                if (checkNode == null || !checkNode.Walkable || checkNode.IsOccupied)
                    return false;
            }
        }
        return true;
    }

    private Vector3 GetFootprintOffset(Vector2Int size)
    {
        return new Vector3((size.x - 1) / 2f, 0f, (size.y - 1) / 2f);
    }

    private void SetAsGhost(GameObject building)
    {
        foreach (var col in building.GetComponentsInChildren<Collider>())
            col.enabled = false;

        foreach (var script in building.GetComponents<MonoBehaviour>())
            if (script != this) script.enabled = false;

        SetGhostColor(Color.green, 0.5f); // ghost look
    }

    private void SetGhostColor(Color baseColor, float alpha)
    {
        if (_originalMaterials == null) return;

        foreach (var mat in _originalMaterials)
        {
            if (mat == null) continue;

            Color color = baseColor;
            color.a = alpha;
            mat.color = color;

            // Force Standard Shader transparency settings
            mat.SetFloat("_Mode", 2); // Fade
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;

            Debug.Log("[Placer] Ghost material updated to transparent");
        }
    }

    private void FinalizePlacement(GameObject building)
    {

        foreach (var col in building.GetComponentsInChildren<Collider>())
            col.enabled = true;

        foreach (var script in building.GetComponents<MonoBehaviour>())
            script.enabled = true;

        foreach (var rend in building.GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in rend.materials)
            {
                // ✅ Fully opaque white
                mat.color = new Color(1f, 1f, 1f, 1f);

                // ✅ Reset shader blending to opaque
                mat.SetFloat("_Mode", 0); // Opaque
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt("_ZWrite", 1);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = -1;
            }
        }

        Debug.Log("[Placer] Final object color and render queue reset.");


    }

    private void CancelPlacement()
    {
        if (_ghostBuilding != null)
            Destroy(_ghostBuilding);

        _ghostBuilding = null;
        _isPlacing = false;
    }

    private Material[] GetAllMaterials(GameObject obj)
    {
        List<Material> mats = new();
        foreach (var rend in obj.GetComponentsInChildren<Renderer>())
            mats.AddRange(rend.materials);

        return mats.ToArray();
    }

    private Vector3 GetMouseWorldPointOnGround()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane ground = new(Vector3.up, Vector3.zero);

        if (ground.Raycast(ray, out float enter))
            return ray.GetPoint(enter);

        return Vector3.zero;
    }
}
