using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitDeployButton : MonoBehaviour
{
    public TMP_Text countText;
    public Image unitIcon;
    private int _remainingCount;
    private GameObject _unitPrefab;

    public void Setup(GameObject prefab, Sprite icon, int count)
    {
        _unitPrefab = prefab;
        _remainingCount = count;

        unitIcon.enabled = true;
        countText.enabled = true;

        unitIcon.sprite = icon;
        countText.text = $"x{_remainingCount}";
    }

    public void DisableSlot()
    {
        unitIcon.enabled = false;         // Hides the unit icon
        countText.enabled = false;        // Hides the count
        _unitPrefab = null;

        GetComponent<Button>().interactable = false; // Prevents clicking
    }

    public void OnClick()
    {
        if (_unitPrefab == null || _remainingCount <= 0) return;

        UnitDeploymentManager.Instance.BeginPlacingUnit(_unitPrefab, this);
        Debug.Log("Ready to deploy unit");
    }

    public void DecreaseCount()
    {
        _remainingCount--;
        countText.text = $"x{_remainingCount}";

        if (_remainingCount <= 0)
            GetComponent<Button>().interactable = false;
    }
}
