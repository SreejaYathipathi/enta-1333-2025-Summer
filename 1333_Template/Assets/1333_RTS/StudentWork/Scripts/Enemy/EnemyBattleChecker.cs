using System.Linq;
using UnityEngine;

public class EnemyBattleChecker : MonoBehaviour
{
    private bool playerUnitSeen = false;        // becomes true once a player unit appears

    void Update()
    {
        // stop if result already decided
        if (GameManager.Instance.CurrentState == GameState.GameOver) return;

        /* ----- count things once per frame ----- */
        var allUnits = FindObjectsOfType<UnitInstance>();
        bool anyPlayers = allUnits.Any(u => u.ArmyID == 0);
        bool anyEnemies = allUnits.Any(u => u.ArmyID == 1);
        bool anyBuildings = FindObjectsOfType<BuildingHealth>()
                            .Any(b => b.ArmyID == 1);

        // mark that we had at least one player unit in the battle
        if (anyPlayers) playerUnitSeen = true;

        /* ---------- defeat ---------- */
        if (playerUnitSeen && !anyPlayers)
        {
            EnemyUIManager.Instance.ShowGameOver(false);   // lose panel
            enabled = false;
            return;
        }

        /* ---------- victory ---------- */
        if (!anyEnemies && !anyBuildings)
        {
            EnemyUIManager.Instance.ShowGameOver(true);    // win panel
            enabled = false;
        }
    }
}
