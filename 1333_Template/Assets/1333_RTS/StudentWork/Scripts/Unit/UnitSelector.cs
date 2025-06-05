using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelector : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private LayerMask _unitLayer;
    [SerializeField] private GridManager _gridManager;

    private ArmyManager _playerArmy;
    private ArmyPathFindingTester _tester;

    private List<UnitInstance> _selectedUnits = new();

    private IEnumerator Start()
    {
        // Wait one frame to ensure ArmyPathFindingTester has initialized
        yield return null;

        _tester = FindObjectOfType<ArmyPathFindingTester>();
        if (_tester == null)
        {
            Debug.LogError("[UnitSelector] ArmyPathFindingTester not found in the scene.");
            yield break;
        }

        _playerArmy = _tester.PlayerArmy;

        if (_playerArmy == null)
        {
            Debug.LogError("[UnitSelector] PlayerArmy is null. Make sure army with ID 0 exists.");
        }
        else
        {
            Debug.Log("[UnitSelector] PlayerArmy successfully assigned.");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (TrySelectUnit())
            {
                Debug.Log("[Select] Unit selected.");
            }
            else
            {
                Debug.Log("[Select] No unit selected.");
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (_selectedUnits.Count > 0)
            {
                TryCommandUnit();
                Debug.Log("[Command] Move command issued.");
            }
            else
            {
                Debug.Log("[Command] No units selected to move.");
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _selectedUnits.Clear();
            Debug.Log("[Select] Selection cleared.");
        }
    }

    bool TrySelectUnit()
    {

        if (_playerArmy == null)
        {
            Debug.LogError("[Select] Cannot check units. PlayerArmy is null.");
            return false;
        }


        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, _unitLayer))
        {
            UnitInstance unit = hit.collider.GetComponent<UnitInstance>();
            if (unit != null && _playerArmy.Units.Contains(unit))
            {
                if (!_selectedUnits.Contains(unit))
                {
                    _selectedUnits.Add(unit);
                    Debug.Log($"[Select] Added {unit.name} to selection");
                }
                return true;
            }
        }
        return false;
    }

    private IEnumerator DestroyWhenUnitArrives(GameObject marker, UnitInstance unit)
    {
        while (unit != null && unit.IsMoving)
        {
            yield return null;
        }

        if (marker != null)
            Destroy(marker);
    }

    void TryCommandUnit()
    {
        if (_selectedUnits.Count == 0) return;

        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GridNode centerNode = _gridManager.GetNodeFromWorldPosition(hit.point);

            if (centerNode == null)
            {
                Debug.LogWarning("[Command] Clicked outside grid");
                return;
            }

            if (!centerNode.Walkable)
            {
                Debug.LogWarning($"[Command] Clicked on unwalkable tile at {centerNode.Name}");
                return;
            }

            Vector3 basePos = centerNode.WorldPosition;

            int count = _selectedUnits.Count;
            int rowSize = Mathf.CeilToInt(Mathf.Sqrt(count));
            float spacing = _gridManager.GridSettings.NodeSize * 1.5f;

            for (int i = 0; i < count; i++)
            {
                int row = i / rowSize;
                int col = i % rowSize;

                Vector3 offset = new Vector3(
                    (col - rowSize / 2f) * spacing,
                    0,
                    (row - rowSize / 2f) * spacing
                );

                Vector3 targetPos = basePos + offset;
                GridNode node = _gridManager.GetNodeFromWorldPosition(targetPos);

                if (node != null && node.Walkable)
                {
                    _selectedUnits[i].MoveTo(node);

                    if (_tester != null)
                    {
                        _tester.SetUnitState(_selectedUnits[i], "Command");
                    }
                }
                else
                {
                    Debug.LogWarning($"[Command] No valid node at offset position for unit {i}");
                }
            }

            Debug.Log($"[Command] Moving {_selectedUnits.Count} units to around {basePos}");

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.transform.position = basePos + Vector3.up * 0.5f;
            marker.transform.localScale = Vector3.one * 0.3f;
            marker.GetComponent<Collider>().enabled = false;

            // Store it and destroy it when unit arrives
            StartCoroutine(DestroyWhenUnitArrives(marker, _selectedUnits[0]));
        }
    }

}
