using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnemyUIManager : MonoBehaviour
{
    public static EnemyUIManager Instance { get; private set; }

    public List<UnitDeployButton> slots; // Assign in Inspector

    public ArmyComposition playerArmy;

    [Header("Confirmation panel")]
    public GameObject confirmPanel;

    [Header("Game-Over")]
    public GameObject winPanel;
    public GameObject losePanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < playerArmy.units.Count)
            {
                var unit = playerArmy.units[i];
                slots[i].Setup(unit.unitTypePrefab.prefab, unit.unitTypePrefab.unitType.Icon, unit.count);

                Debug.Log($"Slot {i}: Unit = {unit.unitTypePrefab.unitType.name}, Prefab = {unit.unitTypePrefab.prefab}, Icon = {unit.unitTypePrefab.unitType.Icon}");
            }
            else
            {
                slots[i].DisableSlot();
            }
        }
    }

    public void OnEndBattleClicked()
    {
        Time.timeScale = 0f;
        confirmPanel.SetActive(true);
    }

    public void OnConfirmYes()
    {
        confirmPanel.SetActive(false);
        GameManager.Instance.EndEnemyBattle();   // original behaviour
    }

    public void OnConfirmNo()
    {
        Time.timeScale = 1f;
        confirmPanel.SetActive(false);           // just resume battle
    }

    public void ShowGameOver(bool won)
    {
        winPanel.SetActive(won);
        losePanel.SetActive(!won);
        Time.timeScale = 0f;

        GameManager.Instance.SetState(GameState.GameOver);
    }
}