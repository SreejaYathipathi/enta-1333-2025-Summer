using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI button inside the Build menu that shows one building item
/// and tells BuildingPlacer which prefab to place when clicked.
/// </summary>
public class BuildingItemButtonUI : MonoBehaviour
{
    public TMP_Text nameText;
    public Image iconImage;
    public TMP_Text priceText;

    private BuildingItemData _itemData;
    private BuildingPlacer _buildingPlacer;

    public void SetData(BuildingItemData item)
    {
        _itemData = item;

        nameText.text = item.itemName;
        iconImage.sprite = item.icon;

        priceText.text = string.Join("  |  ", item.costs
                     .ConvertAll(c => $"{c.amount} {c.type}"));

        bool affordable = ResourceManager.Instance.HasResources(item.costs);
        GetComponent<Button>().interactable = affordable;

        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(OnClickPlaceBuilding);

        _buildingPlacer = FindObjectOfType<BuildingPlacer>();
    }

    /// <summary>Called when the user clicks this button.</summary>
    private void OnClickPlaceBuilding()
    {
        if (_buildingPlacer != null && _itemData.prefab != null)
        {
            _buildingPlacer.SetPrefabToPlace(_itemData);
        }
    }
}
