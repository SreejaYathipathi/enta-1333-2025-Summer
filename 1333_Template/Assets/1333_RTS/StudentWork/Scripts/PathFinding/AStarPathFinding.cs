using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarPathFinding : PathFindingAlgorithm
{
    private GridManager _gridmanager;

    public AStarPathFinding(GridManager grid)
    {
        _gridmanager = grid;
    }

    public override List<GridNode> Findpath(GridNode start, GridNode end)
    {
        List<GridNode> openSet = new List<GridNode>();
        HashSet<GridNode> closedSet = new HashSet<GridNode>();

        _gridmanager.Frontier.Clear();
        _gridmanager.Visited.Clear();
        _gridmanager.Path.Clear();

        openSet.Add(start);
        _gridmanager.Frontier.Add(start);

        start.GCost = 0;
        start.HCost = GetHeuristic(start, end);
        start.Parent = null;

        while (openSet.Count > 0)
        {
            GridNode current = GetLowestFCost(openSet);
            openSet.Remove(current);
            _gridmanager.Frontier.Remove(current);
            closedSet.Add(current);
            _gridmanager.Visited.Add(current);

            if (current == end)
            {
                return ReconstructPath(start, end);
            }

            foreach (GridNode neighbor in _gridmanager.GetNeighbours(current))
            {
                if (closedSet.Contains(neighbor)) continue;

                int tentativeGCost = current.GCost + neighbor.Weight;

                if (!openSet.Contains(neighbor))
                {
                    neighbor.GCost = tentativeGCost;
                    neighbor.HCost = GetHeuristic(neighbor, end);
                    neighbor.Parent = current;

                    openSet.Add(neighbor);
                    _gridmanager.Frontier.Add(neighbor);
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

    public override List<GridNode> Findpath(Vector3 startPos, Vector3 endPos)
    {
        return Findpath(_gridmanager.GetNodeFromWorldPosition(startPos), _gridmanager.GetNodeFromWorldPosition(endPos));
    }

    private int GetHeuristic(GridNode a, GridNode b)
    {
        Vector2 aPos = new Vector2(a.WorldPosition.x, a.WorldPosition.z);
        Vector2 bPos = new Vector2(b.WorldPosition.x, b.WorldPosition.z);
        return Mathf.RoundToInt(Vector2.Distance(aPos, bPos)); // Euclidean
    }

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

        _gridmanager.Path = path;
        return path;
    }
}
