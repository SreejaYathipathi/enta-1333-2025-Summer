using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Abstarct base class for all pathfinding algorithms 

public abstract class PathFindingAlgorithm
{
    /*Finds a path from start to end node
    Parameters:   Start: starting node
                  End: The ending node

    Returns a list of nodes representing the path from start to end.
    */

    public abstract List<GridNode> Findpath(GridNode start, GridNode end);



    public abstract List<GridNode> Findpath(Vector3 start, Vector3 end);
}