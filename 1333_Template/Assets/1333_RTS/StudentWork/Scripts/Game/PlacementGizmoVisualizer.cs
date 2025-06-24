using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementGizmoVisualizer : MonoBehaviour
{
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private Color validColor = new Color(0f, 1f, 0f, 0.5f);   // Semi-transparent green
    [SerializeField] private Color invalidColor = new Color(1f, 0f, 0f, 0.5f); // Semi-transparent red

    private List<GridNode> _gizmoNodes = new();
    private List<bool> _validStates = new();
    private bool _isPlacing = false;

    public void ShowGizmos(List<GridNode> nodes, List<bool> validStates)
    {
        _gizmoNodes = nodes;
        _validStates = validStates;
        _isPlacing = true;
    }

    public void HideGizmos()
    {
        _gizmoNodes.Clear();
        _validStates.Clear();
        _isPlacing = false;
    }

    private void OnDrawGizmos()
    {
        if (!_isPlacing || _gizmoNodes == null) return;

        for (int i = 0; i < _gizmoNodes.Count; i++)
        {
            GridNode node = _gizmoNodes[i];
            if (node == null) continue;

            Gizmos.color = _validStates[i] ? validColor : invalidColor;
            Gizmos.DrawCube(node.WorldPosition, Vector3.one * _gridManager.GridSettings.NodeSize * 0.95f);
        }
    }
}
