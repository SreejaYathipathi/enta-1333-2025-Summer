using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildItemButtonUI : MonoBehaviour
{
    public TMP_Text nameText;
    public Image iconImage;
    public TMP_Text infoText;

    private BuildItemData _itemData;
    private BuildingPlacer _buildingPlacer;

    public void SetData(BuildItemData item)
    {
        _itemData = item;

        nameText.text = item.itemName;
        iconImage.sprite = item.icon;
        infoText.text = $"level {item.requiredLevel}";

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
