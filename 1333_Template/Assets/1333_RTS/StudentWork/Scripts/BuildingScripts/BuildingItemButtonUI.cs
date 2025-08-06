using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingItemButtonUI : MonoBehaviour
{
    public TMP_Text nameText;
    public Image iconImage;
    //public TMP_Text infoText;
    public TMP_Text priceText;

    private BuildingItemData _itemData;
    private BuildingPlacer _buildingPlacer;

    public void SetData(BuildingItemData item)
    {
        _itemData = item;

        nameText.text = item.itemName;
        iconImage.sprite = item.icon;
        //infoText.text = $"level {item.requiredLevel}";

        priceText.text = string.Join("  |  ", item.costs
                     .ConvertAll(c => $"{c.amount} {c.type}"));

        bool affordable = ResourceManager.Instance.HasResources(item.costs);
        GetComponent<Button>().interactable = affordable;

        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(OnClickPlaceBuilding);

        _buildingPlacer = FindObjectOfType<BuildingPlacer>();
    }

    private void OnClickPlaceBuilding()
    {
        if (_buildingPlacer != null && _itemData.prefab != null)
        {
            _buildingPlacer.SetPrefabToPlace(_itemData);
        }
    }
}
