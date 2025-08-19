using UnityEngine;
using UnityEngine.SceneManagement;

public static class LoadingScreen
{
    public static string NextScene = "";

    public static bool RequestLoadMenu = false;
    public static bool RequestUnloadMenu = false;

    public static string MenuSceneName = "MenuOverlay";  // TODO: renommer selon branche jésus

    public static void Load(string sceneName, bool loadMenu = false, bool unloadMenu = false)
    {
        Debug.Log($"[LoadingScreen] Request → LoadingScene (next='{sceneName}', loadMenu={loadMenu}, unloadMenu={unloadMenu})");

        NextScene = sceneName;
        RequestLoadMenu = loadMenu;
        RequestUnloadMenu = unloadMenu;

    #if UNITY_EDITOR
        if (!Application.CanStreamedLevelBeLoaded("LoadingScene"))
            Debug.LogError("‘LoadingScene’ n’est PAS dans le Build Profile.");

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
            Debug.LogError($"La scène cible ‘{sceneName}’ n’est PAS dans le Build Profile.");

        if (loadMenu && !Application.CanStreamedLevelBeLoaded(MenuSceneName))
            Debug.LogWarning($"[TODO] La scène menu ‘{MenuSceneName}’ n’est pas dans le Build Profile ou pas encore créée.");
    #endif
        SceneManager.LoadScene("LoadingScene");
    }

    public static bool IsMenuLoaded()
    {
        var s = SceneManager.GetSceneByName(MenuSceneName);
        return s.IsValid() && s.isLoaded;
    }
}
