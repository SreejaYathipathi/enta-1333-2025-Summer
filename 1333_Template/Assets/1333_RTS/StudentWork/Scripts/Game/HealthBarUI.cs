using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image _BG;   // red
    [SerializeField] private Image _FG;   // green
    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;
        if (_BG != null) _BG.fillAmount = 1f;
        if (_FG != null) _FG.fillAmount = 1f;
        gameObject.SetActive(false);
    }

    public void SetFill(float normalized)
    {
        normalized = Mathf.Clamp01(normalized);
        if (_FG != null) _FG.fillAmount = normalized;
        if (_BG != null) _BG.fillAmount = 1f; // keep background full
    }

    public void Show()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (_cam == null) _cam = Camera.main;
        transform.forward = _cam.transform.forward;
    }
}
