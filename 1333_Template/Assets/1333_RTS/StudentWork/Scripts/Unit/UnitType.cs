using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitType", menuName = "Game/Unit Type")]
public class UnitType : ScriptableObject
{
    [SerializeField] private int _width = 1;
    [SerializeField] private int _height = 1;
    [SerializeField] private int _maxHp = 1;
    [SerializeField] private int _minHp = 1;
    [SerializeField] private int _damage = 1;
    [SerializeField] private int _defense = 1;
    [SerializeField] private AttackType _attackType;
    [SerializeField] private int _range = 1;

    public int Width => _width;
    public int Height => _height;
}
