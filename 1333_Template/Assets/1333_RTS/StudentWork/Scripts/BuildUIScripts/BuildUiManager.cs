using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildUiManager : MonoBehaviour
{
    public GameObject bottomPanel;
    public GameObject itemButtonPrefab;
    public Transform itemContentHolder;
    public List<BuildCategory> buildCategories;
    private bool hasShownDefaultCategory = false;

    public void ToggleBottomPanel()
    {
        bottomPanel.SetActive(!bottomPanel.activeSelf);

        if (bottomPanel.activeSelf)
        {
            ShowCategory("Houses");
            hasShownDefaultCategory = true;
        }
    }

    public void ShowCategory(string categoryName)
    {
        Debug.Log("Trying to show category: " + categoryName);

        foreach (Transform child in itemContentHolder)
        {
            Destroy(child.gameObject);
        }

        BuildCategory category = buildCategories.Find(c => c.categoryName == categoryName);

        if (category == null)
        {
            Debug.LogError("Category NOT FOUND: " + categoryName);
            return;
        }

        foreach (BuildItemData item in category.items)
        {
            Debug.Log($"Creating {item.itemName} | Sprite: {item.icon} | Level: {item.requiredLevel}");

            Debug.Log("Item count in category: " + category.items.Count);

            Debug.Log("Creating UI for item: " + item.itemName);

            GameObject itemBtn = Instantiate(itemButtonPrefab, itemContentHolder);
            itemBtn.GetComponent<ItemButtonUI>().SetData(item);
        }
    }
   
}
