using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ToggleGroup
{
    Build,
    Category,
    Item
}

[RequireComponent(typeof(Button))]
public class UIToggleHighlight : MonoBehaviour
{
    [Header("Highlight")]
    public ToggleGroup group;
    public Color normalColor = Color.white;
    public Color selectedColor = new(1f, 0.85f, 0.4f);

    private static readonly Dictionary<ToggleGroup, UIToggleHighlight> active =
        new Dictionary<ToggleGroup, UIToggleHighlight>();

    private Button btn;
    private Graphic gfx;

    private void Awake()
    {
        btn = GetComponent<Button>();
        gfx = btn.targetGraphic;
        btn.onClick.AddListener(OnClicked);
        SetTint(normalColor);
    }

    private void OnClicked()
    {
        if (group == ToggleGroup.Build && active.TryGetValue(group, out var cur) && cur == this)
        {
            Deselect();
            active.Remove(group);
            return;
        }

        if (active.TryGetValue(group, out var prev) && prev && prev != this)
            prev.Deselect();

        Select();
        active[group] = this;
    }

    public void Select() => SetTint(selectedColor);
    public void Deselect() => SetTint(normalColor);
    private void SetTint(Color c) { if (gfx) gfx.color = c; }

    public static void ClearGroup(ToggleGroup g)
    {
        if (active.TryGetValue(g, out var a) && a) a.Deselect();
        active.Remove(g);
    }
}
