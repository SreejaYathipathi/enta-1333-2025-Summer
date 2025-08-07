using UnityEngine;

/// <summary>
/// Helper functions for converting between linear slider values and decibel (dB) values for audio UI.
/// </summary>
public static class AudioUtils
{
    // The minimum decibel value (Unity uses this as mute)
    private const float MinDb = -80f; // Mute in Unity

    // The maximum decibel value (full volume)
    private const float MaxDb = 0f;   // Full volume

    /// <summary>
    /// Convert a linear slider value (0-1) to a decibel (dB) value.
    /// </summary>
    /// <param name="slider">Slider value between 0 and 1.</param>
    /// <returns>Decibel value, clamped to Unity's audio range.</returns>
    public static float SliderToDb(float slider)
    {
        const float minDb = -80f; // Mute
        const float maxDb = 0f;   // Full volume
        float clamped = Mathf.Clamp(slider, 0.0001f, 1f);
        float db = Mathf.Log10(clamped) * 20f;
        return Mathf.Clamp(db, minDb, maxDb);
    }

    /// <summary>
    /// Convert a decibel (dB) value back to a linear slider value (0-1).
    /// </summary>
    /// <param name="db">Decibel value, typically from -80 (mute) to 0 (full).</param>
    /// <returns>Slider value between 0 and 1.</returns>
    public static float DbToSlider(float db) =>
        Mathf.InverseLerp(MinDb, MaxDb, db);
}
