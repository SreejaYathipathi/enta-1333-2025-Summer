using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridNode
{
    public string Name;
    public Vector3 WorldPosition;
    public bool Walkable;
    public int Weight;

    public TerrainType TerrainType;

    // A* variables
    public int GCost;
    public int HCost;
    public int FCost => GCost + HCost;
    public GridNode Parent;
}
