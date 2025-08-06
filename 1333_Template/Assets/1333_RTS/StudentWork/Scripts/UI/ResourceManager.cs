using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    [Header("UI")]
    public TMP_Text WwoodText;
    public TMP_Text StoneText;
    public TMP_Text CrystalText;
    public TMP_Text AquaText;
    public TMP_Text AmethystText;
    public TMP_Text RubyText;

    private int _wood;
    private int _stone;
    private int _crystal;
    private int _aqua;
    private int _amethyst;
    private int _ruby;

    private void Awake()
    {
        Instance = this;
    }

    public int GetWood() => _wood;
    public int GetStone() => _stone;
    public int GetCrystal() => _crystal;
    public int GetAqua() => _aqua;
    public int GetAmethyst() => _amethyst;
    public int GetRuby() => _ruby;

    public void SetWood(int value) { _wood = value; WwoodText.text = value.ToString(); }
    public void SetStone(int value) { _stone = value; StoneText.text = value.ToString(); }
    public void SetCrystal(int value) { _crystal = value; CrystalText.text = value.ToString(); }
    public void SetAqua(int value) { _aqua = value; AquaText.text = value.ToString(); }
    public void SetAmethyst(int value) { _amethyst = value; AmethystText.text = value.ToString(); }
    public void SetRuby(int value) { _ruby = value; RubyText.text = value.ToString(); }

    public void AddResource(ResourceType type, int amount)
    {
        switch (type)
        {
            case ResourceType.Wood: _wood += amount; WwoodText.text = _wood.ToString(); break;
            case ResourceType.Stone: _stone += amount; StoneText.text = _stone.ToString(); break;
            case ResourceType.Crystal: _crystal += amount; CrystalText.text = _crystal.ToString(); break;
            case ResourceType.Aqua: _aqua += amount; AquaText.text = _aqua.ToString(); break;
            case ResourceType.Amethyst: _amethyst += amount; AmethystText.text = _amethyst.ToString(); break;
            case ResourceType.Ruby: _ruby += amount; RubyText.text = _ruby.ToString(); break;
        }
    }

    public bool HasResourceCheck(ResourceType type, int amount)
    {
        switch (type)
        {
            case ResourceType.Wood: return _wood >= amount;
            case ResourceType.Stone: return _stone >= amount;
            case ResourceType.Crystal: return _crystal >= amount;
            case ResourceType.Aqua: return _aqua >= amount;
            case ResourceType.Amethyst: return _amethyst >= amount;
            case ResourceType.Ruby: return _ruby >= amount;
            default: return false;
        }
    }

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
            case ResourceType.Ruby: SetRuby(_ruby - amount); break;
        }
        return true;
    }

    public bool HasResources(List<CostEntry> list)
    {
        foreach (var c in list)
            if (!HasResourceCheck(c.type, c.amount)) return false;
        return true;
    }

    public void SpendResources(List<CostEntry> list)
    {
        foreach (var c in list)
            SpendResourceCheck(c.type, c.amount);       // we already wrote SpendResource()
    }
}
