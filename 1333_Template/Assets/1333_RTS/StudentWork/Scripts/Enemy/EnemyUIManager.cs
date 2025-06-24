using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnemyUIManager : MonoBehaviour
{

    public List<UnitDeployButton> slots; // Assign in Inspector

    public ArmyComposition playerArmy;

    private void Start()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < playerArmy.units.Count)
            {
                var unit = playerArmy.units[i];
                slots[i].Setup(unit.unitTypePrefab.prefab, unit.unitTypePrefab.unitType.Icon, unit.count);

                //slots[i].Setup(unit.unitTypePrefab.prefab, unit.unitTypePrefab.icon, unit.count);
            }
            else
            {
                slots[i].DisableSlot();
            }
        }
    }

    public void OnEndBattleClicked()
    {
        SceneManager.LoadScene("PlayerScene"); // Match your exact scene name
    }
}