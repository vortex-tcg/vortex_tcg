using UnityEngine;
using System;



[System.Serializable]
public class AppConfigs
{
    public AppConfig dev;
    public AppConfig prod;
}

[System.Serializable]
public class AppConfig
{
    public string apiBaseUrl;
    public string gameHubUrl;
    public string baseUrl;
}

public static class ConfigLoader
{
    private static AppConfigs config;

    public static AppConfig Load()
    {
        if (config != null) {
            #if UNITY_EDITOR
                return config.dev;
            #else
                return config.prod;
            #endif
        }

        TextAsset configText = Resources.Load<TextAsset>("application-properties");
        if (configText == null)
        {
            Debug.LogError("Failed to load configuration file: application-properties");
            return null;
        }

        config = JsonUtility.FromJson<AppConfigs>(configText.text);

        #if UNITY_EDITOR
            return config.dev;
        #else
            return config.prod;
        #endif
    }

    public static string BuildGameHubUrl(AppConfig cfg)
    {
        if (cfg == null)
            return null;

        if (!string.IsNullOrWhiteSpace(cfg.gameHubUrl))
            return cfg.gameHubUrl;

        if (string.IsNullOrWhiteSpace(cfg.apiBaseUrl))
            return null;

        if (string.IsNullOrWhiteSpace(cfg.baseUrl))
            return null;

        string baseUrl = cfg.gameHubUrl.TrimEnd('/');
        if (baseUrl.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
            baseUrl = baseUrl.Substring(0, baseUrl.Length - 4);

        return baseUrl.TrimEnd('/') + "/hubs/game";
    }
}