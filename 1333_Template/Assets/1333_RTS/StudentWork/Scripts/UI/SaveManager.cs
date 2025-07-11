using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SaveManager
{
    public static void SaveSlot(int slot, string playerName)
    {
        PlayerPrefs.SetString($"Slot{slot}_Name", playerName);

        long now = System.DateTime.Now.Ticks;
        PlayerPrefs.SetString($"Slot{slot}_LastPlayed", now.ToString());
        PlayerPrefs.Save();
    }

    public static string GetSlotName(int slot)
    {
        return PlayerPrefs.GetString($"Slot{slot}_Name", "");
    }

    public static int GetLastPlayed(int slot)
    {
        string savedTicks = PlayerPrefs.GetString($"Slot{slot}_LastPlayed", "");
        if (string.IsNullOrEmpty(savedTicks)) return -1;

        long ticks;
        if (long.TryParse(savedTicks, out ticks))
        {
            System.DateTime savedTime = new System.DateTime(ticks);
            System.TimeSpan span = System.DateTime.Now - savedTime;
            return (int)span.TotalDays;
        }
        return -1;
    }

    public static void DeleteSlot(int slot)
    {
        PlayerPrefs.DeleteKey($"Slot{slot}_Name");
        PlayerPrefs.DeleteKey($"Slot{slot}_LastPlayed");
    }

    public static bool SlotExists(int slot)
    {
        string name = PlayerPrefs.GetString($"Slot{slot}_Name", "");
        return !string.IsNullOrEmpty(name);
    }
}
