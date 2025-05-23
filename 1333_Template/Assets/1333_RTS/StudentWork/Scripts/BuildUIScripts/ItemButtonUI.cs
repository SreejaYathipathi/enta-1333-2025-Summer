using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemButtonUI : MonoBehaviour
{
    public TMP_Text nameText;
    public Image iconImage;
    public TMP_Text infoText;

    public void SetData(BuildItemData item)
    {
        nameText.text = item.itemName;
        iconImage.sprite = item.icon;
        infoText.text = $"level {item.requiredLevel}";
    }
}
