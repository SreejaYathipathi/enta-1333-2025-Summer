using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Binds a UI Slider to an AudioChannel in AudioManager at runtime.
/// Prevents broken serialized event references when scenes reload (singleton survives).
/// </summary>
[RequireComponent(typeof(Slider))]
public class AudioVolumeSlider : MonoBehaviour
{
    [SerializeField] private AudioChannel _channel = AudioChannel.Master;

    [Tooltip("If true, resync slider value from AudioManager every time this object is enabled.")]
    [SerializeField] private bool _syncOnEnable = true;

    private Slider _slider;

    /// <summary>
    /// Setup slider reference and event listener.
    /// </summary>
    private void Awake()
    {
        _slider = GetComponent<Slider>();
        _slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    /// <summary>
    /// Initialize the slider UI to match AudioManager at start.
    /// </summary>
    private void Start()
    {
        SyncFromManager();
    }

    /// <summary>
    /// Optionally resync slider value from AudioManager when re-enabled.
    /// </summary>
    private void OnEnable()
    {
        if (_syncOnEnable)
            SyncFromManager();
    }

    /// <summary>
    /// Clean up event listener when this object is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        if (_slider != null)
            _slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    /// <summary>
    /// Handler for slider value changes. Updates AudioManager.
    /// </summary>
    private void OnSliderValueChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetVolume(_channel, value);
    }

    /// <summary>
    /// Sets the slider UI to match the current AudioManager volume (does not trigger event).
    /// </summary>
    public void SyncFromManager()
    {
        if (AudioManager.Instance == null || _slider == null)
            return;

        float v = AudioManager.Instance.GetVolume01(_channel);
        _slider.SetValueWithoutNotify(v);
    }
}
