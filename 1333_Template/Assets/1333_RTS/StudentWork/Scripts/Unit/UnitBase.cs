using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class UnitBase : MonoBehaviour
{
    // Reference to the unit's type definition (contains stats like size, damage, etc.)
    [SerializeField] protected UnitType _unitType;

    // Public property to get the width of the unit (defaults to 1 if _unitType is null)
    public virtual int Width => _unitType != null ? _unitType.Width : 1;

    // Public property to get the height of the unit (defaults to 1 if _unitType is null)
    public virtual int Height => _unitType != null ? _unitType.Height : 1;

    // Abstract method that must be implemented to move the unit to a specific grid node
    public abstract void MoveTo(GridNode targetNode);

    public virtual int MaxHP => _unitType.maxHp;
    public virtual int MinHP => _unitType.minHp;
    public virtual int Defense => _unitType.defense;
    public virtual int Damage => _unitType.damage;
    public virtual int Range => _unitType.range;
}
