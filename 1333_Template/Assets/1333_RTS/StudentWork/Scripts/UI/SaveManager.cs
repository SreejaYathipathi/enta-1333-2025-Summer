using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapObstacleData
{
    public string prefabName;
    public float posX, posY, posZ;
    public float rotY;
}

[System.Serializable]
public class MapData
{
    public List<MapObstacleData> obstacles = new List<MapObstacleData>();
}

[System.Serializable]
public class PlayerSceneData
{
    public int wood, stone, crystal, aqua, amethyst, ruby;
    public int completedWaves;
    public List<BuildingData> buildings = new();
    public MapData mapData = new MapData();
}

[System.Serializable]
public class BuildingData
{
    public string prefabName;
    public float posX, posY, posZ;
    public float rotY;
    public float health;
}

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

    public static void SavePlayerScene(PlayerSceneData data, int slot)
    {
        string json = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString($"PlayerSceneSave_{slot}", json);
        PlayerPrefs.Save();
    }

    public static PlayerSceneData LoadPlayerScene(int slot)
    {
        string key = $"PlayerSceneSave_{slot}";
        if (!PlayerPrefs.HasKey(key)) return null;
        return JsonUtility.FromJson<PlayerSceneData>(PlayerPrefs.GetString(key));
    }

    public static int GetProfileImageIndex(int slot)
    {
        return PlayerPrefs.GetInt($"Slot{slot}_Image", -1);
    }

    public static string GetSlotName(int slot)
    {
        return PlayerPrefs.GetString($"Slot{slot}_Name", "");
    }

    public static int GetLastPlayed(int slot)
    {
        string savedTicks = PlayerPrefs.GetString($"Slot{slot}_LastPlayed", "");
        return GetDaysAgo(savedTicks);
    }

    public static int GetCreatedDaysAgo(int slot)
    {
        string savedTicks = PlayerPrefs.GetString($"Slot{slot}_Created", "");
        return GetDaysAgo(savedTicks);
    }

    private static int GetDaysAgo(string ticksString)
    {
        if (string.IsNullOrEmpty(ticksString)) return -1;
        if (!long.TryParse(ticksString, out long ticks)) return -1;

        System.DateTime savedTime = new System.DateTime(ticks);
        System.TimeSpan span = System.DateTime.Now - savedTime;
        return (int)span.TotalDays;
    }

    public static void DeleteSlot(int slot)
    {
        PlayerPrefs.DeleteKey($"Slot{slot}_Name");
        PlayerPrefs.DeleteKey($"Slot{slot}_LastPlayed");
        PlayerPrefs.DeleteKey($"Slot{slot}_Created");
        PlayerPrefs.DeleteKey($"Slot{slot}_Image");
        PlayerPrefs.DeleteKey($"PlayerSceneSave_{slot}");
    }

    public static bool SlotExists(int slot)
    {
        return !string.IsNullOrEmpty(PlayerPrefs.GetString($"Slot{slot}_Name", ""));
    }
}