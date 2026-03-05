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
        // Force reload every time for debugging
        config = null;
        
        TextAsset configText = Resources.Load<TextAsset>("application-properties");
        if (configText == null)
        {
            Debug.LogError("Failed to load configuration file: application-properties");
            return null;
        }

        config = JsonUtility.FromJson<AppConfigs>(configText.text);
        
        #if UNITY_EDITOR
            Debug.Log("[ConfigLoader] Loaded DEV config: " + config.dev.gameHubUrl);
            return config.dev;
        #else
            Debug.Log("[ConfigLoader] Loaded PROD config: " + config.prod.gameHubUrl);
            return config.prod;
        #endif
    }

    public static string BuildGameHubUrl(AppConfig cfg)
    {
        if (cfg == null)
        {
            Debug.LogWarning("[ConfigLoader] cfg is NULL");
            return null;
        }

        if (!string.IsNullOrWhiteSpace(cfg.gameHubUrl))
        {
            Debug.Log($"[ConfigLoader] Using gameHubUrl: {cfg.gameHubUrl}");
            return cfg.gameHubUrl;
        }

        Debug.LogWarning("[ConfigLoader] gameHubUrl is empty, attempting fallback logic");
        
        if (string.IsNullOrWhiteSpace(cfg.baseUrl))
        {
            Debug.LogError("[ConfigLoader] baseUrl is also empty");
            return null;
        }

        string baseUrl = cfg.baseUrl.TrimEnd('/');
        if (baseUrl.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
            baseUrl = baseUrl.Substring(0, baseUrl.Length - 4);

        string fallbackUrl = baseUrl.TrimEnd('/') + "/hubs/game";
        Debug.Log($"[ConfigLoader] Using fallback URL: {fallbackUrl}");
        return fallbackUrl;
    }
}