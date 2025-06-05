using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyManager
{
    public int ArmyID;

    public bool IsPlayer => ArmyID == 0;

    public List<UnitBase> Units = new List<UnitBase>();

    //public List<BuildingBase> Buildings = new List<BuildingBase>();

    public GridManager GridManager;

    public void MoveAllUnitsTo(Vector3 worldPosition)
    {
        foreach (var unit in Units)
        {
            unit.MoveTo(GridManager.GetNodeFromWorldPosition(worldPosition));
        }
    }

    public void MoveAllUnitsTo(GridNode node)
    {
        foreach (var unit in Units)
        {
            unit.MoveTo(node);
        }
    }
}
