using UnityEngine;

public static class SaveService
{
    private const float DefaultSaveIntervalSeconds = 0.25f;

    private static bool isDirty;
    private static float lastSaveTime = -1000f;

    public static bool HasKey(string key)
    {
        return PlayerPrefs.HasKey(key);
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }

    public static float GetFloat(string key, float defaultValue = 0f)
    {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }

    public static string GetString(string key, string defaultValue = "")
    {
        return PlayerPrefs.GetString(key, defaultValue);
    }

    public static void SetInt(string key, int value, bool saveImmediately = false)
    {
        PlayerPrefs.SetInt(key, value);
        MarkDirty(saveImmediately);
    }

    public static void SetFloat(string key, float value, bool saveImmediately = false)
    {
        PlayerPrefs.SetFloat(key, value);
        MarkDirty(saveImmediately);
    }

    public static void SetString(string key, string value, bool saveImmediately = false)
    {
        PlayerPrefs.SetString(key, value);
        MarkDirty(saveImmediately);
    }

    public static void SaveIfDirty(float minIntervalSeconds = DefaultSaveIntervalSeconds)
    {
        if (!isDirty)
        {
            return;
        }

        if (Time.unscaledTime - lastSaveTime < minIntervalSeconds)
        {
            return;
        }

        SaveNow();
    }

    public static void SaveNow()
    {
        PlayerPrefs.Save();
        lastSaveTime = Time.unscaledTime;
        isDirty = false;
    }

    private static void MarkDirty(bool saveImmediately)
    {
        isDirty = true;
        if (saveImmediately)
        {
            SaveNow();
        }
    }
}
