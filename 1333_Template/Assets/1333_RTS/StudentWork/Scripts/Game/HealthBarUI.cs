using UnityEngine;
using UnityEngine.UI;

/// World-space health bar UI that faces the camera.
public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image _BG;   // red
    [SerializeField] private Image _FG;   // green
    private Camera _cam;

    // Cache main camera and init bar at full.
    private void Awake()
    {
        _cam = Camera.main;
        if (_BG != null) _BG.fillAmount = 1f;
        if (_FG != null) _FG.fillAmount = 1f;
        gameObject.SetActive(false);
    }

    // Sets the fill amount (0-1) for the foreground bar.
    public void SetFill(float normalized)
    {
        normalized = Mathf.Clamp01(normalized);
        if (_FG != null) _FG.fillAmount = normalized;
        if (_BG != null) _BG.fillAmount = 1f; // keep background full
    }

    // Makes the bar visible.
    public void Show()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    // Hides the bar.
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // Rotate the bar to face the camera every frame.
    private void LateUpdate()
    {
        if (_cam == null) _cam = Camera.main;
        transform.forward = _cam.transform.forward;
    }
}
