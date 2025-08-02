using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ResourceType { Wood, Stone, Crystal, Aqua, Amethyst, Ruby } // <-- restored

public class ObstacleCuttable : MonoBehaviour
{
    private ObstacleSpawner _spawner;
    private GridNode _node;

    [Header("Cut Settings")]
    public int requiredCuts = 4;
    public ResourceType resourceType = ResourceType.Wood;
    public int resourceAmount = 5;

    private int _cutCount = 0;
    private bool isDestroyed = false;

    private Renderer[] _renderers;
    private Color[] _originalColors;
    private bool _isFlashing = false;

    // Hard lock
    private UnitInstance _assignedUnit = null;

    private void Start()
    {
        // Get all renderers in this obstacle
        _renderers = GetComponentsInChildren<Renderer>();
        _originalColors = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
        {
            _originalColors[i] = _renderers[i].material.color;
        }
    }

    public void Init(ObstacleSpawner spawner, GridNode node)
    {
        _spawner = spawner;
        _node = node;
    }

    /// <summary>
    /// Try to assign unit exclusively. Returns false if already taken.
    /// </summary>
    public bool TryAssign(UnitInstance unit)
    {
        return _assignedUnit == null || _assignedUnit == unit;
    }

    public void Unassign(UnitInstance unit)
    {
        if (_assignedUnit == unit)
            _assignedUnit = null;
    }

    public void Cut()
    {
        if (isDestroyed) return;

        _cutCount++;
        //Debug.Log($"Obstacle hit {_cutCount}/{requiredCuts}");

        if (!_isFlashing)
            StartCoroutine(FlashRed());

        if (_cutCount >= requiredCuts)
        {
            isDestroyed = true;

            ResourceManager.Instance.AddResource(resourceType, resourceAmount);

            if (_node == null)
            {
                GridManager grid = FindObjectOfType<GridManager>();
                if (grid != null)
                    _node = grid.GetNodeFromWorldPosition(transform.position);
            }
            if (_node != null)
            {
                _node.IsOccupied = false;
                _node.Walkable = true;
            }

            _assignedUnit = null; // allow respawn reuse
            if (_spawner != null)
            {
                _spawner.HandleCut(gameObject, _node);
            }
            else
            {
                FreeNode();
                Destroy(gameObject);
            }
        }
    }

    private void FreeNode()
    {
        if (_node != null)
        {
            _node.IsOccupied = false;
            _node.Walkable = true;
        }
    }

    private void OnDestroy()
    {
        FreeNode();
    }

    private IEnumerator FlashRed()
    {
        _isFlashing = true;

        // Change to red
        foreach (var rend in _renderers)
            rend.material.color = Color.red;

        yield return new WaitForSeconds(0.1f); // fast flash duration

        // Restore original color
        for (int i = 0; i < _renderers.Length; i++)
            _renderers[i].material.color = _originalColors[i];

        _isFlashing = false;
    }


    private void OnMouseDown()
    {
        if (SceneManager.GetActiveScene().name != "PlayerScene") return;

        Debug.Log($"Clicked obstacle: {name}");

        // If already has a unit assigned, stop right here
        if (_assignedUnit != null)
        {
            Debug.Log($"Obstacle {name} already assigned to {_assignedUnit.name}");
            return;
        }

        UnitInstance[] allUnits = GameObject.FindObjectsOfType<UnitInstance>();
        UnitInstance closestUnit = null;
        float closestDistance = Mathf.Infinity;

        foreach (var unit in allUnits)
        {
            if (unit.Mode != ControlMode.Manual || unit.IsMoving) continue;

            float dist = Vector3.Distance(unit.transform.position, transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestUnit = unit;
            }
        }

        if (closestUnit == null)
        {
            Debug.Log("No idle manual unit available.");
            return;
        }

        // **Hard lock before calling MoveToAndCut**
        _assignedUnit = closestUnit;

        Debug.Log($"Assigning {_assignedUnit.name} to cut {name}");
        _assignedUnit.MoveToAndCut(this);
    }
}
