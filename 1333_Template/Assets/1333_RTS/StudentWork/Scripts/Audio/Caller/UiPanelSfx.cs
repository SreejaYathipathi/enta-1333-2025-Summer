using UnityEngine;

public class UIPanelOpenSfx : MonoBehaviour
{
    [Tooltip("Clip name exactly as it appears in SfxClipDatabase")]
    [SerializeField] private string openSfxKey = "UIPanelOpen_01";

    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    private void OnEnable()
    {
        AudioManager.Instance?.Play2dSfx(openSfxKey, volume);
    }
}
