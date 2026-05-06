using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public static class LoadingScreen
{
    private static LoadingRequest _req;
    private static LoadingRequest Req
        => _req ? _req : (_req = Resources.Load<LoadingRequest>("LoadingRequest"));

    public static void Load(string sceneName, bool loadMenu = false, bool unloadMenu = false)
    {
        Req.targetScene  = NormalizeSceneName(sceneName);
        Req.loadMenu     = loadMenu;
        Req.unloadMenu   = unloadMenu;

    #if UNITY_EDITOR
        if (!Application.CanStreamedLevelBeLoaded("LoadingScene"))
            Debug.LogError("‘LoadingScene’ n’est pas dans le Build Profile.");
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
            Debug.LogError($"La scène cible ‘{sceneName}’ n’est pas dans le Build Profile.");
        if (loadMenu && !Application.CanStreamedLevelBeLoaded(Req.menuSceneName))
            Debug.LogWarning($"[TODO] Scène menu ‘{Req.menuSceneName}’ introuvable / non ajoutée.");
    #endif
        SceneManager.LoadScene("LoadingScene");
    }

    private static string NormalizeSceneName(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return sceneName;

        return Path.GetFileNameWithoutExtension(sceneName).Replace("\\", "/").Split('/')[^1];
    }

    public static LoadingRequest GetRequest() => Req;
}
