using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// Handles battle UI for the enemy scene: deploy slots, confirm dialogs, rewards, and game-over screens.
public class EnemyUIManager : MonoBehaviour
{
    public static EnemyUIManager Instance { get; private set; }

    public List<UnitDeployButton> slots; // Assign in Inspector

    public ArmyComposition playerArmy;

    [Header("Confirmation panel")]
    public GameObject confirmPanel;

    // Panels shown on win / loss.
    [Header("Game-Over")]
    public GameObject winPanel;
    public GameObject losePanel;

    // Reward generation on victory.
    [Header("Reward System")]
    public ResourceRewardTable rewardTable;
    public ResourceIconLibrary iconLibrary;
    public GameObject costPrefab;
    public Transform costLayout;
    public int minRewards = 2;
    public int maxRewards = 5;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    // Fill deploy slots with units from playerArmy.
    private void Start()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < playerArmy.units.Count)
            {
                var unit = playerArmy.units[i];
                slots[i].Setup(unit.unitTypePrefab.prefab, unit.unitTypePrefab.unitType.Icon, unit.count);

                //Debug.Log($"Slot {i}: Unit = {unit.unitTypePrefab.unitType.name}, Prefab = {unit.unitTypePrefab.prefab}, Icon = {unit.unitTypePrefab.unitType.Icon}");
            }
            else
            {
                slots[i].DisableSlot();
            }
        }
    }

    // Pause and open the “End battle?” dialog.
    public void OnEndBattleClicked()
    {
        Time.timeScale = 0f;
        confirmPanel.SetActive(true);
    }

    // Player confirmed ending the battle.
    public void OnConfirmYes()
    {
        confirmPanel.SetActive(false);
        GameManager.Instance.EndEnemyBattle();   // original behaviour
    }

    // Player cancelled; resume play
    public void OnConfirmNo()
    {
        Time.timeScale = 1f;
        confirmPanel.SetActive(false);           // just resume battle
    }

    // Show win/lose screen and give rewards if won.
    public void ShowGameOver(bool won)
    {
        winPanel.SetActive(won);
        losePanel.SetActive(!won);

        if (won) GrantRandomRewards();

        Time.timeScale = 0f;

        GameManager.Instance.SetState(GameState.GameOver);
    }

    // Picks random resources from rewardTable and adds them to inventory.
    private void GrantRandomRewards()
    {
        /* clear old UI */
        foreach (Transform c in costLayout) Destroy(c.gameObject);

        /* build pool of types */
        List<ResourceType> pool = new();
        foreach (var row in rewardTable.rows) pool.Add(row.type);

        /* random count this time */
        int give = Random.Range(minRewards, maxRewards + 1);
        give = Mathf.Min(give, pool.Count);

        for (int i = 0; i < give; i++)
        {
            ResourceType type = rewardTable.GetRandomType();
            while (!pool.Contains(type))                     // avoid duplicates
                type = rewardTable.GetRandomType();
            pool.Remove(type);

            if (!rewardTable.TryGetRow(type, out var row)) continue;
            int amount = Random.Range(row.minAmount, row.maxAmount + 1);

            ResourceManager.Instance.AddResource(type, amount);

            GameManager.Instance.AddPendingReward(type, amount);

            GameObject go = Instantiate(costPrefab, costLayout);
            go.SetActive(true);

            Image icon = go.transform.Find("Icon").GetComponent<Image>();
            TMP_Text amountT = go.transform.Find("Amount").GetComponent<TMP_Text>();

            icon.sprite = iconLibrary.GetIcon(type);
            amountT.text = $"+{amount}";
        }
    }
}
