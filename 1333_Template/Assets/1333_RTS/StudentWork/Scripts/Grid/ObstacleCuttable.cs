using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObstacleCuttable : MonoBehaviour
{
    private ObstacleSpawner _spawner;
    private GridNode _node;
    private int _cutCount = 0;
    public int requiredCuts = 4;

    public void Init(ObstacleSpawner spawner, GridNode node)
    {
        _spawner = spawner;
        _node = node;
    }

    public void Cut()
    {
        _cutCount++;

        Debug.Log($"Obstacle hit {_cutCount}/{requiredCuts}");

        if (_cutCount >= requiredCuts)
        {
            _spawner.HandleCut(gameObject, _node);
        }
    }

    private void OnMouseDown()
    {
        if (SceneManager.GetActiveScene().name != "PlayerScene") return;

        UnitInstance[] allUnits = GameObject.FindObjectsOfType<UnitInstance>();
        UnitInstance closestUnit = null;
        float closestDistance = Mathf.Infinity;

        foreach (var unit in allUnits)
        {
            if (unit.Mode != ControlMode.Manual || unit.IsMoving)
                continue;

            float dist = Vector3.Distance(unit.transform.position, transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestUnit = unit;
            }
        }

        if (closestUnit != null)
        {
            closestUnit.MoveToAndCut(this);
        }
        else
        {
            Debug.Log("No idle manual unit available to cut.");
        }
    }
}
