using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleCuttable : MonoBehaviour
{
    private ObstacleSpawner _spawner;
    private GridNode _node;

    public void Init(ObstacleSpawner spawner, GridNode node)
    {
        _spawner = spawner;
        _node = node;
    }

    public void Cut()
    {
        _spawner.HandleCut(gameObject, _node);
    }

    // Optional: demo input (you can replace with axe tool, UI, etc.)
    private void OnMouseDown()
    {
        Cut();
    }
}
