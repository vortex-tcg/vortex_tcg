using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LoadingController : MonoBehaviour
{
    public TMP_Text progressText;          
    [Range(0f, 2f)] public float minShowTime = 0.5f;

    private void Start()
    {
        Debug.Log("[LoadingController] start ok, next = " + LoadingScreen.NextScene);
        StartCoroutine(LoadNext());
    }

    private IEnumerator LoadNext()
    {
        yield return null;

        var op = SceneManager.LoadSceneAsync(LoadingScreen.NextScene);
        if (op == null)
        {
            Debug.LogError("[LoadingController] LoadSceneAsync null (nom de scène invalide ?)");
            yield break;
        }
        op.allowSceneActivation = false;

        float elapsed = 0f;
        while (op.progress < 0.9f) 
        {
            if (progressText)
            {
                float pct = Mathf.Clamp01(op.progress / 0.9f) * 100f;
                progressText.text = $"Chargement… {pct:0}%";
            }
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        while (elapsed < minShowTime) { elapsed += Time.unscaledDeltaTime; yield return null; }

    
        if (LoadingScreen.RequestUnloadMenu && LoadingScreen.IsMenuLoaded())
        {
            Debug.Log("[LoadingController] Unload menu overlay…");
            yield return SceneManager.UnloadSceneAsync(LoadingScreen.MenuSceneName);
        }

        if (LoadingScreen.RequestLoadMenu)
        {
        #if UNITY_EDITOR
            if (!Application.CanStreamedLevelBeLoaded(LoadingScreen.MenuSceneName))
            {
                Debug.LogWarning($"[TODO] Menu '{LoadingScreen.MenuSceneName}' introuvable. Ajoutez la scène / Build Profile.");
            }
            else
        #endif
            if (!LoadingScreen.IsMenuLoaded())
            {
                Debug.Log("[LoadingController] Load menu overlay (additive) …");
                var menuOp = SceneManager.LoadSceneAsync(LoadingScreen.MenuSceneName, LoadSceneMode.Additive);
                if (menuOp != null) { while (!menuOp.isDone) yield return null; }
            }
        }

        Debug.Log("[LoadingController] activation scène cible");
        op.allowSceneActivation = true;
    }
}
