using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// This attribute allows you to create instances of UnitType as assets via the Unity Editor's "Create" menu
[CreateAssetMenu(fileName = "TerrainType", menuName = "Game/TerrainType")]

public class TerrainType : ScriptableObject
{
    [SerializeField] private string _terrainName = "Default";
    [SerializeField] private Color _gizmoColor = Color.green;
    [SerializeField] private bool _walkable = true;
    [SerializeField] private int _movementCost = 1;
    [SerializeField] private GameObject _prefab;

    public string TerrainName => _terrainName;
    public Color GizmoColor => _gizmoColor;
    public bool IsWalkable => _walkable;
    public int MovementCost => _movementCost;
    public GameObject Prefab => _prefab;

}
