using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds all units that belong to one army (player or enemy)  
/// and provides group-movement helpers.
/// </summary>
public class ArmyManager
{
    public int ArmyID;

    // Convenience flag for quick “is this the player?” checks
    public bool IsPlayer => ArmyID == 0;

    public List<UnitBase> Units = new List<UnitBase>();

    public GridManager GridManager;

    /// <summary>
    /// Orders every unit in the list to move to a world-space position.
    /// </summary>
    public void MoveAllUnitsTo(Vector3 worldPosition)
    {
        foreach (var unit in Units)
        {
            unit.MoveTo(GridManager.GetNodeFromWorldPosition(worldPosition));
        }
    }

    /// <summary>
    /// Orders every unit to move to a specific grid node.
    /// </summary>
    public void MoveAllUnitsTo(GridNode node)
    {
        foreach (var unit in Units)
        {
            unit.MoveTo(node);
        }
    }
}
