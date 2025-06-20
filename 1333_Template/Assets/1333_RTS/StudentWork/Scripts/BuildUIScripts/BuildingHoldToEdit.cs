using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingHoldToEdit : MonoBehaviour
{
    [SerializeField] private float holdDuration = 0.5f;

    private float holdTimer = 0f;
    private bool isHolding = false;

    void Update()
    {
        if (IsPointerOverThisBuilding())
        {
            if (Input.GetMouseButton(0))
            {
                holdTimer += Time.deltaTime;
                if (!isHolding && holdTimer >= holdDuration)
                {
                    Debug.Log("You are in edit mode");
                    isHolding = true;
                    BuildingEditUI.Instance.ShowUI(this.gameObject);
                }
            }
            else
            {
                ResetHold();
            }
        }
        else
        {
            ResetHold();
        }
    }

    private bool IsPointerOverThisBuilding()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            return hit.collider != null && hit.collider.transform.root.gameObject == this.gameObject;
        }
        return false;
    }

    private void ResetHold()
    {
        holdTimer = 0f;
        isHolding = false;
    }
}
