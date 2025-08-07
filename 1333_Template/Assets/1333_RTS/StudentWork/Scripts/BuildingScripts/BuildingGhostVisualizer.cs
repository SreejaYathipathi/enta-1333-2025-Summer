using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Turns a model into a semi-transparent “ghost” for placement previews,
/// then restores it to normal once confirmed.
/// </summary>
public static class BuildingGhostVisualizer
{

    private static Dictionary<GameObject, Material[]> originalMats = new();
    private static Dictionary<GameObject, Color> lastAppliedColor = new();

    /// <summary>Convert a spawned prefab into a green ghost.</summary>
    public static void MakeGhost(GameObject obj)
    {
        // Store its materials on first use
        if (!originalMats.ContainsKey(obj))
        {
            var mats = new List<Material>();
            foreach (var rend in obj.GetComponentsInChildren<Renderer>())
                mats.AddRange(rend.materials);
            originalMats[obj] = mats.ToArray();
        }

        foreach (var col in obj.GetComponentsInChildren<Collider>())
            col.enabled = false;

        foreach (var script in obj.GetComponents<MonoBehaviour>())
            script.enabled = false;

        SetGhostColor(obj, Color.green, 0.5f);
    }

    /// <summary>Restore materials and render settings back to opaque white.</summary>
    public static void MakeReal(GameObject obj)
    {
        if (lastAppliedColor.ContainsKey(obj))
            lastAppliedColor.Remove(obj);

        foreach (var rend in obj.GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in rend.materials)
            {
                mat.color = Color.white;
                mat.SetFloat("_Mode", 0);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt("_ZWrite", 1);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = -1;
            }
        }
    }

    /// <summary>Apply a tinted transparent look to all renderers.</summary>
    public static void SetGhostColor(GameObject obj, Color color, float alpha)
    {
        Color target = new(color.r, color.g, color.b, alpha);

        // If the same color was applied last frame, skip
        if (lastAppliedColor.TryGetValue(obj, out var lastColor))
        {
            if (ApproximatelyEqualColor(lastColor, target))
                return;
        }

        lastAppliedColor[obj] = target;

        if (!originalMats.TryGetValue(obj, out var mats)) return;

        foreach (var rend in obj.GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in rend.materials)
            {
                mat.color = target;
                mat.SetFloat("_Mode", 2);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
        }
    }

    // Helper to compare two colours with a tolerance (skip subtle differences)
    private static bool ApproximatelyEqualColor(Color a, Color b, float tolerance = 0.01f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance &&
               Mathf.Abs(a.a - b.a) < tolerance;
    }
}
