using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelector : MonoBehaviour
{
    // Reference to the main camera
    [SerializeField] private Camera _camera;

    // Layer mask to identify selectable units
    [SerializeField] private LayerMask _unitLayer;

    // Reference to the GridManager for node lookup
    [SerializeField] private GridManager _gridManager;


    // Player's army data
    private ArmyManager _playerArmy;

    // Tester used to initialize the player's army
    private ArmyPathFindingTester _tester;

    // List of currently selected units
    public List<UnitInstance> _selectedUnits = new();

    private IEnumerator Start()
    {
        // Wait one frame to ensure ArmyPathFindingTester has initialized
        yield return null;

        _tester = GameObject.FindAnyObjectByType<ArmyPathFindingTester>();
        if (_tester == null)
        {
            //Debug.LogError("[UnitSelector] ArmyPathFindingTester not found in the scene.");
            yield break;
        }

        _playerArmy = _tester.PlayerArmy;

        if (_playerArmy == null)
        {
            //Debug.LogError("[UnitSelector] PlayerArmy is null. Make sure army with ID 0 exists.");
        }
        else
        {
            //Debug.Log("[UnitSelector] PlayerArmy successfully assigned.");
        }
    }

    void Update()
    {

        // Left-click to select unit
        if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.U))
        {
            if (TrySelectUnit())
            {
                //Debug.Log("[Select] Unit selected.");
            }
            else
            {
                //Debug.Log("[Select] No unit selected.");
            }
        }


        // Right-click to issue move command
        if (Input.GetMouseButtonDown(1))
        {
            if (_selectedUnits.Count > 0)
            {
                CommandSelectedUnits();
                //Debug.Log("[Command] Move command issued.");
            }
            else
            {
                //Debug.Log("[Command] No units selected to move.");
            }
        }

        // Escape key to clear selection
        if (Input.GetKeyDown(KeyCode.C))
        {
            _selectedUnits.Clear();
            //Debug.Log("[Select] Selection cleared.");
        }
    }


    // Attempts to select a unit the player clicked on
    bool TrySelectUnit()
    {

        if (_playerArmy == null)
        {
            //Debug.LogError("[Select] Cannot check units. PlayerArmy is null.");
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
                    //Debug.Log($"[Select] Added {unit.name} to selection");
                }
                else
                {
                    //Debug.LogWarning("Not added to selection");
                }
                    return true;
            }
        }
        return false;
    }


    // Sends selected units to a clicked position
    private void CommandSelectedUnits()
    {
        if (_camera == null || _gridManager == null) return;

        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane ground = new(Vector3.up, Vector3.zero);

        if (ground.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            GridNode node = _gridManager.GetNodeFromWorldPosition(hitPoint);
            if (!node.Walkable)
            {
                //Debug.Log("SelectionManager: Target node is not walkable.");
                return;
            }

            foreach (UnitBase unit in _selectedUnits)
                unit.MoveTo(node);
        }
    }
}
