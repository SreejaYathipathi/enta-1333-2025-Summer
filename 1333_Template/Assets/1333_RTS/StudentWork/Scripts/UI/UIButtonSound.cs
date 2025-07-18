using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public enum ButtonType { Menu, Building }
    public ButtonType type;

    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.Instance.PlayHover();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (type == ButtonType.Menu)
            AudioManager.Instance.PlayMenuClick();
        else if (type == ButtonType.Building)
            AudioManager.Instance.PlayBuildingClick();
    }
}
