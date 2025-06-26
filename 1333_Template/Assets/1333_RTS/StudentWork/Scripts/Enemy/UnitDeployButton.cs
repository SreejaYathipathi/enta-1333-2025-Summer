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
        /* Debug.Log($"[Setup] Setting up button: Icon={icon}, Count={count}, Prefab={prefab.name}");

         _unitPrefab = prefab;
         _remainingCount = count;

         unitIcon.sprite = icon;
         countText.text = $"x{_remainingCount}";

         // NEW: Ensure objects are active
         unitIcon.gameObject.SetActive(true);
         countText.gameObject.SetActive(true);
         gameObject.SetActive(true);

         unitIcon.enabled = true;
         countText.enabled = true;
         GetComponent<Button>().interactable = true;*/


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

    public void DisableSlot()
    {

        Debug.Log("[Disable] Disabling this slot");

        unitIcon.enabled = false;         // Hides the unit icon
        countText.enabled = false;        // Hides the count
        _unitPrefab = null;

        GetComponent<Button>().interactable = false; // Prevents clicking
    }

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

    public void DecreaseCount()
    {
        _remainingCount--;
        countText.text = $"x{_remainingCount}";

        if (_remainingCount <= 0)
            GetComponent<Button>().interactable = false;
    }
}
