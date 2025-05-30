using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarPathFinding : PathFindingAlgorithm
{
    private GridManager gridManager;

    public AStarPathFinding(GridManager manager)
    {
        gridManager = manager;
    }

    public override List<GridNode> Findpath(GridNode start, GridNode end)
    {
        List<GridNode> openSet = new List<GridNode> { start };
        HashSet<GridNode> closedSet = new HashSet<GridNode>();

        gridManager.Path = new List<GridNode>();
        gridManager.Frontier = new List<GridNode>();
        gridManager.Visited = new HashSet<GridNode>();

        start.GCost = 0;
        start.HCost = GetDistance(start, end);
        start.Parent = null;

        while (openSet.Count > 0)
        {
            GridNode currentNode = GetLowestFCostNode(openSet);

            if (currentNode == end)
            {
                List<GridNode> finalPath = RetracePath(start, end);
                gridManager.Path = finalPath;
                Debug.Log($"Found end! {finalPath.Count}");
                return finalPath;
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);
            gridManager.Visited.Add(currentNode);

            foreach (GridNode neighbour in gridManager.GetNeighbours(currentNode))
            {
                if (closedSet.Contains(neighbour) || !neighbour.Walkable)
                    continue;

                int tentativeG = currentNode.GCost + neighbour.Weight;

                if (!openSet.Contains(neighbour) || tentativeG < neighbour.GCost)
                {
                    neighbour.GCost = tentativeG;
                    neighbour.HCost = GetDistance(neighbour, end);
                    neighbour.Parent = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                        gridManager.Frontier.Add(neighbour);
                    }
                }
            }
        }

        return null; // No path found
    }

    private int GetDistance(GridNode a, GridNode b)
    {
        Vector3 diff = a.WorldPosition - b.WorldPosition;
        return Mathf.RoundToInt(diff.magnitude);
    }

    private GridNode GetLowestFCostNode(List<GridNode> nodes)
    {
        GridNode best = nodes[0];
        foreach (GridNode node in nodes)
        {
            if (node.FCost < best.FCost || (node.FCost == best.FCost && node.HCost < best.HCost))
                best = node;
        }
        return best;
    }

    private List<GridNode> RetracePath(GridNode start, GridNode end)
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

    public override List<GridNode> Findpath(Vector3 startPos, Vector3 endPos)
    {
        Debug.Log("A* Findpath called");
        // Convert positions to grid coordinates if needed
        GridNode startNode = gridManager.GetNodeFromWorldPosition(startPos);
        GridNode endNode = gridManager.GetNodeFromWorldPosition(endPos);
        return Findpath(startNode, endNode);
    }
}
