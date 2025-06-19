using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridNode
{
    public bool IsOccupied = false;

    public string Name;

    public int GridX;
    public int GridY;
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
