using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Button in the deploy bar that lets the player spawn a limited-count unit.
public class UnitDeployButton : MonoBehaviour
{
    public TMP_Text countText;
    public Image unitIcon;
    private int _remainingCount;
    public int RemainingCount => _remainingCount;
    private GameObject _unitPrefab;

    // Fill the button with data from ArmyComposition.
    public void Setup(GameObject prefab, Sprite icon, int count)
    {
        Debug.Log($"[Setup] Setting up button: Icon={icon}, Count={count}, Prefab={prefab.name}");

        _unitPrefab = prefab;
        _remainingCount = count;

        unitIcon.sprite = icon;
        countText.text = $"x{_remainingCount}";

        unitIcon.gameObject.SetActive(true);
        countText.gameObject.SetActive(true);
        gameObject.SetActive(true);

        unitIcon.enabled = true;
        countText.enabled = true;
        GetComponent<Button>().interactable = true;

        GetComponent<Button>().onClick.RemoveAllListeners(); // Optional: prevents duplicate calls
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    // Hide visuals and disable the button for empty slots.
    public void DisableSlot()
    {

        Debug.Log("[Disable] Disabling this slot");

        unitIcon.enabled = false;         // Hides the unit icon
        countText.enabled = false;        // Hides the count
        _unitPrefab = null;

        GetComponent<Button>().interactable = false; // Prevents clicking
    }

    // Called when the player clicks this button.
    public void OnClick()
    {
        if (_unitPrefab == null)
        {
            Debug.Log("There is no unit");

            return;
        }

        if (_remainingCount <= 0)
        {
            Debug.Log("Unit count is 0");

            return;
        }


        UnitDeploymentManager.Instance.BeginPlacingUnit(_unitPrefab, this);
        Debug.Log("Ready to deploy unit");
    }

    // Decrement count after a unit is placed and disable when empty.
    public void DecreaseCount()
    {
        _remainingCount--;
        countText.text = $"x{_remainingCount}";

        if (_remainingCount <= 0)
        {
            GetComponent<Button>().interactable = false;
            Debug.Log("Unit count reached zero — button disabled.");
        }
    }
}
