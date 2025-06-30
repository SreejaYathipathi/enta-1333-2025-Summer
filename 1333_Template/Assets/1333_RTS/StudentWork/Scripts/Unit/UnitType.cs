using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// This attribute allows you to create instances of UnitType as assets via the Unity Editor's "Create" menu
[CreateAssetMenu(fileName = "UnitType", menuName = "Game/Unit Type")]
public class UnitType : ScriptableObject
{
    //Data for units
    [SerializeField] private int _width = 1;
    [SerializeField] private int _height = 1;
    [SerializeField] private int _maxHp = 1;
    [SerializeField] private int _minHp = 1;
    [SerializeField] private int _damage = 1;
    [SerializeField] private int _defense = 1;
    [SerializeField] private AttackType _attackType;
    [SerializeField] private int _range = 1;
    [SerializeField] private Sprite _icon;

    [SerializeField]
    private List<BuildingPurpose> _targetPreference;

    // Public getter for width
    public int Width => _width;
    // Public getter for height
    public int Height => _height;

    public int maxHp => _maxHp; 
    public int minHp => _minHp; 
    public int damage => _damage; 
    public int defense => _defense; 
    public int range => _range;
    public Sprite Icon => _icon;
    public List<BuildingPurpose> TargetPreference => _targetPreference;

}
