using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Plays UI hover / click SFX for the attached Button.
/// Attach this component to any UI Button.
/// </summary>
[RequireComponent(typeof(Button))]
public class UIButtonSfx : MonoBehaviour,
                            IPointerEnterHandler,
                            IPointerClickHandler
{
    /* ------------------------------------------------------------------ */
    /*  Inspector                                                         */
    /* ------------------------------------------------------------------ */
    [Header("SFX Keys")]
    [SerializeField] private string _hoverKey = "UIButtonHover_01";
    [SerializeField] private string _clickKey = "UIButtonClick_01";

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float _volume = 1f;

    /* ------------------------------------------------------------------ */
    /*  Cache                                                             */
    /* ------------------------------------------------------------------ */
    private Button _button;

    /* ------------------------------------------------------------------ */
    /*  Unity lifecycle                                                   */
    /* ------------------------------------------------------------------ */
    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnButtonClicked);
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OnButtonClicked);
    }

    /* ------------------------------------------------------------------ */
    /*  UI Events                                                         */
    /* ------------------------------------------------------------------ */
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_button.interactable) return;     // ignore disabled buttons
        AudioManager.Instance.Play2dSfx(_hoverKey, _volume);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        /* handled via onClick listener; kept for completeness */
    }

    private void OnButtonClicked()
    {
        if (!_button.interactable) return;
        AudioManager.Instance.Play2dSfx(_clickKey, 0.8f);
    }
}