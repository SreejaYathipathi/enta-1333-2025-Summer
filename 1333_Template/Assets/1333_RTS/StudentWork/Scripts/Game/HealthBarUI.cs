using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    private Transform _target;
    private Camera _cam;
    private CanvasGroup _canvasGroup;
    [SerializeField] private Image _FG;
    [SerializeField] private Image _BG;

    public void AttachTo(Transform target)
    {
        _target = target;
        _BG.gameObject.SetActive(true);
    }

    public void SetFill(float normalized)
    {
        if (_FG != null)
        {
            _FG.fillAmount = normalized;
            _canvasGroup.alpha = (normalized < 1f && normalized > 0f) ? 1f : 0f;
            Debug.Log($"[HealthBarUI] Fill updated: {normalized}");
        }
    }

    private void Awake()
    {
        _cam = Camera.main;
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        _BG.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        Vector3 screenPos = _cam.WorldToScreenPoint(_target.position + Vector3.up * 2f);
        transform.position = screenPos;
    }
}