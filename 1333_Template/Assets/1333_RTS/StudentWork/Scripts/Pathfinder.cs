using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PathFinder : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;

    private List<GridNode> currentPath = new List<GridNode>();

    private void Start()
    {
        // Call pathfinding at runtime from (0,0) to (9,9)
        FindPath(new Vector2Int(0, 0), new Vector2Int(9, 9));
    }

    public void FindPath(Vector2Int start, Vector2Int goal)
    {
        currentPath.Clear();

        //Debug walkability of start & goal
        var startNode = gridManager.GetNode(start.x, start.y);
        var goalNode = gridManager.GetNode(goal.x, goal.y);

        Debug.Log("Start walkable: " + (startNode?.Walkable ?? false));
        Debug.Log("Goal walkable: " + (goalNode?.Walkable ?? false));

        if (startNode == null || goalNode == null || !startNode.Value.Walkable || !goalNode.Value.Walkable)
        {
            Debug.LogWarning("Invalid start or goal for pathfinding.");
            return;
        }

        Queue<Vector2Int> front = new Queue<Vector2Int>();
        front.Enqueue(start);

        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        cameFrom[start] = start;

        while (front.Count > 0)
        {
            Vector2Int current = front.Dequeue();

            if (current == goal)
                break;

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (!cameFrom.ContainsKey(neighbor))
                {
                    GridNode? node = gridManager.GetNode(neighbor.x, neighbor.y);
                    if (node != null && node.Value.Walkable)
                    {
                        front.Enqueue(neighbor);
                        cameFrom[neighbor] = current;
                    }
                }
            }
        }

        // Reconstruct path
        if (cameFrom.ContainsKey(goal))
        {
            Vector2Int current = goal;
            while (current != start)
            {
                GridNode? node = gridManager.GetNode(current.x, current.y);
                currentPath.Add(node.Value);
                current = cameFrom[current];
            }

            currentPath.Reverse();
            Debug.Log("Path found. Length: " + currentPath.Count);

            foreach (var node in currentPath)
            {
                Debug.Log("Step: " + node.Name + " at " + node.WorldPosition);
                
            }
        }
        else
        {
            Debug.LogWarning("No path found!");
        }
    }

    private List<Vector2Int> GetNeighbors(Vector2Int coord)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (var dir in directions)
        {
            Vector2Int next = coord + dir;
            if (gridManager.GetNode(next.x, next.y) != null)
            {
                neighbors.Add(next);
            }
        }

        return neighbors;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = Color.yellow;
        Handles.DrawLine(Vector3.zero, Vector3.one * 5f); // Should show in Scene

        if (currentPath == null || currentPath.Count < 2)
            return;

        Handles.color = Color.red;

        for (int i = 0; i < currentPath.Count - 1; i++)
        {
            Vector3 from = currentPath[i].WorldPosition + Vector3.up * 0.1f;
            Vector3 to = currentPath[i + 1].WorldPosition + Vector3.up * 0.1f;

            Handles.DrawAAPolyLine(5f, from, to); // 5f is thickness
        }
    }
#endif
}
