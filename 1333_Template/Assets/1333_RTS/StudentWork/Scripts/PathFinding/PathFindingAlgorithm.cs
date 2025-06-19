using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Abstarct base class for all pathfinding algorithms 

public abstract class PathFindingAlgorithm
{
    public abstract List<GridNode> Findpath(GridNode start, GridNode end);



    public abstract List<GridNode> Findpath(Vector3 start, Vector3 end);
}