using UnityEngine;
using System;

[System.Serializable]
public class AppConfig
{
    public string apiBaseUrl;
    public string gameHubUrl;
}

public static class ConfigLoader
{
    private static AppConfig config;

    public static AppConfig Load()
    {
        if (config != null)
            return config;

        TextAsset configText = Resources.Load<TextAsset>("application-properties");
        if (configText == null)
        {
            Debug.LogError("Failed to load configuration file: application-properties");
            return null;
        }

        config = JsonUtility.FromJson<AppConfig>(configText.text);
        return config;
    }

    public static string BuildGameHubUrl(AppConfig cfg)
    {
        if (cfg == null)
            return null;

        if (!string.IsNullOrWhiteSpace(cfg.gameHubUrl))
            return cfg.gameHubUrl;

        if (string.IsNullOrWhiteSpace(cfg.apiBaseUrl))
            return null;

        string baseUrl = cfg.apiBaseUrl.TrimEnd('/');
        if (baseUrl.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
            baseUrl = baseUrl.Substring(0, baseUrl.Length - 4);

        return baseUrl.TrimEnd('/') + "/hubs/game";
    }
}