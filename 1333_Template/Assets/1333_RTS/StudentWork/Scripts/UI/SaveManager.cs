using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Holds position / rotation data for a single map obstacle.
[System.Serializable]
public class MapObstacleData
{
    public string prefabName;
    public float posX, posY, posZ;
    public float rotY;
}

// Stores a list of obstacles placed on the map.
[System.Serializable]
public class MapData
{
    public List<MapObstacleData> obstacles = new List<MapObstacleData>();
}

// Top-level container for everything saved in PlayerScene.
[System.Serializable]
public class PlayerSceneData
{
    public int wood, stone, crystal, aqua, amethyst, emerald;
    public int completedWaves;
    public List<BuildingData> buildings = new();
    public MapData mapData = new MapData();
    public int xp;
    public int level;
}

// Serialized info for one placed building.
[System.Serializable]
public class BuildingData
{
    public string prefabName;
    public float posX, posY, posZ;
    public float rotY;
    public float health;
}

// Static helper that stores / loads save-slot metadata and PlayerScene JSON.
public static class SaveManager
{
    private const string PLAYER_SCENE_KEY = "PlayerSceneSave";

    public static void SaveSlot(int slot, string playerName, int imageIndex)
    {
        bool isNew = !SlotExists(slot);

        PlayerPrefs.SetString($"Slot{slot}_Name", playerName);
        PlayerPrefs.SetInt($"Slot{slot}_Image", imageIndex);

        long now = System.DateTime.Now.Ticks;
        PlayerPrefs.SetString($"Slot{slot}_LastPlayed", now.ToString());

        if (isNew)
            PlayerPrefs.SetString($"Slot{slot}_Created", now.ToString());

        PlayerPrefs.Save();
    }

    // Serialize PlayerSceneData to JSON and store it.
    public static void SavePlayerScene(PlayerSceneData data, int slot)
    {
        string json = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString($"PlayerSceneSave_{slot}", json);
        PlayerPrefs.Save();
    }

    // Load PlayerSceneData for the given slot (or null if none)
    public static PlayerSceneData LoadPlayerScene(int slot)
    {
        string key = $"PlayerSceneSave_{slot}";
        if (!PlayerPrefs.HasKey(key)) return null;
        return JsonUtility.FromJson<PlayerSceneData>(PlayerPrefs.GetString(key));
    }

    // Retrieve stored avatar index.
    public static int GetProfileImageIndex(int slot)
    {
        return PlayerPrefs.GetInt($"Slot{slot}_Image", -1);
    }

    // Retrieve stored profile name.
    public static string GetSlotName(int slot)
    {
        return PlayerPrefs.GetString($"Slot{slot}_Name", "");
    }

    // Days since the slot was last played.
    public static int GetLastPlayed(int slot)
    {
        string savedTicks = PlayerPrefs.GetString($"Slot{slot}_LastPlayed", "");
        return GetDaysAgo(savedTicks);
    }

    // Days since the slot was created.
    public static int GetCreatedDaysAgo(int slot)
    {
        string savedTicks = PlayerPrefs.GetString($"Slot{slot}_Created", "");
        return GetDaysAgo(savedTicks);
    }

    // Helper that converts stored ticks to “days ago”.
    private static int GetDaysAgo(string ticksString)
    {
        if (string.IsNullOrEmpty(ticksString)) return -1;
        if (!long.TryParse(ticksString, out long ticks)) return -1;

        System.DateTime savedTime = new System.DateTime(ticks);
        System.TimeSpan span = System.DateTime.Now - savedTime;
        return (int)span.TotalDays;
    }

    // Delete all data associated with a slot.
    public static void DeleteSlot(int slot)
    {
        PlayerPrefs.DeleteKey($"Slot{slot}_Name");
        PlayerPrefs.DeleteKey($"Slot{slot}_LastPlayed");
        PlayerPrefs.DeleteKey($"Slot{slot}_Created");
        PlayerPrefs.DeleteKey($"Slot{slot}_Image");
        PlayerPrefs.DeleteKey($"PlayerSceneSave_{slot}");
    }

    // Returns true if the slot has a name stored (i.e., exists).
    public static bool SlotExists(int slot)
    {
        return !string.IsNullOrEmpty(PlayerPrefs.GetString($"Slot{slot}_Name", ""));
    }
}