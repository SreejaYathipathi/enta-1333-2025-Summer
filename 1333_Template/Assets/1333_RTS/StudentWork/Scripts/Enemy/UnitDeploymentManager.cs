using UnityEngine;

public class UnitDeploymentManager : MonoBehaviour
{
    public static UnitDeploymentManager Instance;
    private GameObject _currentUnitPrefab;
    private UnitDeployButton _activeButton;

    private void Awake()
    {
        Instance = this;
    }

    public void BeginPlacingUnit(GameObject prefab, UnitDeployButton button)
    {
        _currentUnitPrefab = prefab;
        _activeButton = button;
    }

    private void Update()
    {
        if (_currentUnitPrefab == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = GetMouseWorldPoint();
            GridNode node = FindObjectOfType<GridManager>().GetNodeFromWorldPosition(worldPos);
            if (node != null && node.Walkable && !node.IsOccupied)
            {
                GameObject go = Instantiate(_currentUnitPrefab, node.WorldPosition + Vector3.up * 0.2f, Quaternion.identity);
                node.IsOccupied = true;

                if (_activeButton != null)
                {
                    _activeButton.DecreaseCount();

                    if (_activeButton.RemainingCount <= 0)
                    {
                        // Stop allowing further placement
                        _currentUnitPrefab = null;
                        _activeButton = null;
                    }
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    _currentUnitPrefab = null;
                }
                Debug.Log("Unit deployed!");
            }
        }
    }

    private Vector3 GetMouseWorldPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane ground = new(Vector3.up, Vector3.zero);
        return ground.Raycast(ray, out float enter) ? ray.GetPoint(enter) : Vector3.zero;
    }
}
