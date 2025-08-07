using UnityEngine;
using TMPro;
using System.Collections.Generic;

// Tracks all player resources and updates their UI counters.
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    [Header("UI")]
    public TMP_Text WwoodText;
    public TMP_Text StoneText;
    public TMP_Text CrystalText;
    public TMP_Text AquaText;
    public TMP_Text AmethystText;
    public TMP_Text EmeraldText;

    private int _wood;
    private int _stone;
    private int _crystal;
    private int _aqua;
    private int _amethyst;
    private int _emerald;

    private void Awake()
    {
        Instance = this;
    }

    public int GetWood() => _wood;
    public int GetStone() => _stone;
    public int GetCrystal() => _crystal;
    public int GetAqua() => _aqua;
    public int GetAmethyst() => _amethyst;
    public int GetEmerald() => _emerald;

    public void SetWood(int value) { _wood = value; WwoodText.text = value.ToString(); }
    public void SetStone(int value) { _stone = value; StoneText.text = value.ToString(); }
    public void SetCrystal(int value) { _crystal = value; CrystalText.text = value.ToString(); }
    public void SetAqua(int value) { _aqua = value; AquaText.text = value.ToString(); }
    public void SetAmethyst(int value) { _amethyst = value; AmethystText.text = value.ToString(); }
    public void SetEmerald(int value) { _emerald = value; EmeraldText.text = value.ToString(); }

    // add one resource and refresh UI
    public void AddResource(ResourceType type, int amount)
    {
        switch (type)
        {
            case ResourceType.Wood: _wood += amount; WwoodText.text = _wood.ToString(); break;
            case ResourceType.Stone: _stone += amount; StoneText.text = _stone.ToString(); break;
            case ResourceType.Crystal: _crystal += amount; CrystalText.text = _crystal.ToString(); break;
            case ResourceType.Aqua: _aqua += amount; AquaText.text = _aqua.ToString(); break;
            case ResourceType.Amethyst: _amethyst += amount; AmethystText.text = _amethyst.ToString(); break;
            case ResourceType.Emerald: _emerald += amount; EmeraldText.text = _emerald.ToString(); break;
        }
    }

    // check if we have enough of a single resource
    public bool HasResourceCheck(ResourceType type, int amount)
    {
        switch (type)
        {
            case ResourceType.Wood: return _wood >= amount;
            case ResourceType.Stone: return _stone >= amount;
            case ResourceType.Crystal: return _crystal >= amount;
            case ResourceType.Aqua: return _aqua >= amount;
            case ResourceType.Amethyst: return _amethyst >= amount;
            case ResourceType.Emerald: return _emerald >= amount;
            default: return false;
        }
    }

    // spend a single resource if available
    public bool SpendResourceCheck(ResourceType type, int amount)
    {
        if (!HasResourceCheck(type, amount)) return false;

        switch (type)
        {
            case ResourceType.Wood: SetWood(_wood - amount); break;
            case ResourceType.Stone: SetStone(_stone - amount); break;
            case ResourceType.Crystal: SetCrystal(_crystal - amount); break;
            case ResourceType.Aqua: SetAqua(_aqua - amount); break;
            case ResourceType.Amethyst: SetAmethyst(_amethyst - amount); break;
            case ResourceType.Emerald: SetEmerald(_emerald - amount); break;
        }
        return true;
    }

    // check a list of costs
    public bool HasResources(List<CostEntry> list)
    {
        foreach (var c in list)
            if (!HasResourceCheck(c.type, c.amount)) return false;
        return true;
    }

    // spend a list of costs
    public void SpendResources(List<CostEntry> list)
    {
        foreach (var c in list)
            SpendResourceCheck(c.type, c.amount);       // we already wrote SpendResource()
    }
}
