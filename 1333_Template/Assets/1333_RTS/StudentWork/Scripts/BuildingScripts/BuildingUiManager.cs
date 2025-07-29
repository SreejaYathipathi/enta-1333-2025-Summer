using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildingUiManager : MonoBehaviour
{
    public GameObject bottomPanel;
    public GameObject itemButtonPrefab;
    public Transform itemContentHolder;
    public List<BuildingCategory> buildCategories;
    private bool _hasShownDefaultCategory = false;

    [SerializeField] private BuildingEditLogic _editLogic;

    public void ToggleBottomPanel()
    {

        bottomPanel.SetActive(!bottomPanel.activeSelf);

        if (bottomPanel.activeSelf)
        {
            ShowCategory("Houses");
            _hasShownDefaultCategory = true;

        }
    }

    public void ShowCategory(string categoryName)
    {
        Debug.Log("Trying to show category: " + categoryName);

        foreach (Transform child in itemContentHolder)
        {
            Destroy(child.gameObject);
        }

        BuildingCategory category = buildCategories.Find(c => c.categoryName == categoryName);

        if (category == null)
        {
            Debug.LogError("Category NOT FOUND: " + categoryName);
            return;
        }

        foreach (BuildingItemData item in category.items)
        {
            Debug.Log($"Creating {item.itemName} | Sprite: {item.icon} | Level: {item.requiredLevel}");

            Debug.Log("Item count in category: " + category.items.Count);

            Debug.Log("Creating UI for item: " + item.itemName);

            GameObject itemBtn = Instantiate(itemButtonPrefab, itemContentHolder);
            itemBtn.GetComponent<BuildingItemButtonUI>().SetData(item);
        }
    }

}
