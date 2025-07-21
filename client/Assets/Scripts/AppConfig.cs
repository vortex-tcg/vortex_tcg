using UnityEngine;

[System.Serializable]
public class AppConfig
{
    public string apiBaseUrl;
}

public static class ConfigLoader
{
    private static AppConfig config;

    public static AppConfig Load()
    {
        if (config != null) return config;

        TextAsset configText = Resources.Load<TextAsset>("application-properties");
        config = JsonUtility.FromJson<AppConfig>(configText.text);
        return config;
    }
}
