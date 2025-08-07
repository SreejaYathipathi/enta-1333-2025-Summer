using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AStarpath-finding that works on GridManager.
/// </summary>
public class AStarPathFinding : PathFindingAlgorithm
{
    private GridManager _gridmanager;

    public GridManager GridManager => _gridmanager;

    public AStarPathFinding(GridManager grid)
    {
        _gridmanager = grid;
    }

    // Main entry find a path between two nodes.
    public override List<GridNode> Findpath(GridNode start, GridNode end)
    {

        foreach (var node in _gridmanager.GetAllNodes())
        {
            node.Parent = null;
            node.GCost = 0;
            node.HCost = 0;
        }

        List<GridNode> openSet = new List<GridNode>();
        HashSet<GridNode> closedSet = new HashSet<GridNode>();

        openSet.Add(start);

        start.GCost = 0;
        start.HCost = GetHeuristic(start, end);
        start.Parent = null;

        while (openSet.Count > 0)
        {
            // Pick node with lowest F = G + H
            GridNode current = GetLowestFCost(openSet);
            openSet.Remove(current);
            closedSet.Add(current);

            // Reached the goal, rebuild path
            if (current == end)
            {
                return ReconstructPath(start, end);
            }

            // Check neighbors
            foreach (GridNode neighbor in _gridmanager.GetNeighbours(current))
            {

                if (!neighbor.Walkable || neighbor.IsOccupied)
                    continue;

                if (closedSet.Contains(neighbor)) continue;

                int tentativeGCost = current.GCost + neighbor.Weight;

                if (!openSet.Contains(neighbor))
                {
                    neighbor.GCost = tentativeGCost;
                    neighbor.HCost = GetHeuristic(neighbor, end);
                    neighbor.Parent = current;

                    openSet.Add(neighbor);
                }
                else if (tentativeGCost < neighbor.GCost)
                {
                    neighbor.GCost = tentativeGCost;
                    neighbor.Parent = current;
                }
            }
        }

        return null;
    }

    // Convenience overload that takes world positions.
    public override List<GridNode> Findpath(Vector3 startPos, Vector3 endPos)
    {
        return Findpath(_gridmanager.GetNodeFromWorldPosition(startPos), _gridmanager.GetNodeFromWorldPosition(endPos));
    }

    // Heuristic cost (Euclidean distance on X-Z plane)
    private int GetHeuristic(GridNode a, GridNode b)
    {
        Vector2 aPos = new Vector2(a.WorldPosition.x, a.WorldPosition.z);
        Vector2 bPos = new Vector2(b.WorldPosition.x, b.WorldPosition.z);
        return Mathf.RoundToInt(Vector2.Distance(aPos, bPos)); // Euclidean
    }

    // Helper: pick the node with the lowest F cost
    private GridNode GetLowestFCost(List<GridNode> nodes)
    {
        GridNode lowest = nodes[0];
        foreach (var node in nodes)
        {
            if (node.FCost < lowest.FCost || (node.FCost == lowest.FCost && node.HCost < lowest.HCost))
            {
                lowest = node;
            }
        }
        return lowest;
    }

    // Walk back from end → start to build the path list
    private List<GridNode> ReconstructPath(GridNode start, GridNode end)
    {
        List<GridNode> path = new List<GridNode>();
        GridNode current = end;

        while (current != start)
        {
            path.Add(current);
            current = current.Parent;
        }
        path.Add(start);
        path.Reverse();

        return path;
    }
}
