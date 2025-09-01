using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LoadingController : MonoBehaviour
{
    public TMP_Text progressText; 
    public CanvasGroup cg;          

    private LoadingRequest req;

    void Start()
    {
        req = Resources.Load<LoadingRequest>("LoadingRequest"); 
        if (!req) { Debug.LogError("LoadingRequest introuvable dans Resources."); return; }

        if (req.useFade && cg) { cg.alpha = 0f; StartCoroutine(Fade(cg, 0f, 1f, req.fadeDuration)); }
        StartCoroutine(LoadNext());
    }

    IEnumerator LoadNext()
    {
        yield return null; 
        var op = SceneManager.LoadSceneAsync(req.targetScene);
        if (op == null) { Debug.LogError("LoadSceneAsync null. Nom de scène invalide ?"); yield break; }
        op.allowSceneActivation = false;

        float elapsed = 0f;
        while (op.progress < 0.9f)
        {
            elapsed += Time.unscaledDeltaTime;
            if (progressText)
            {
                float pct = Mathf.Clamp01(op.progress / 0.9f) * 100f;
                progressText.text = $"Chargement… {pct:0}%";
            }
            yield return null;
        }

        while (elapsed < req.minShowTime) { elapsed += Time.unscaledDeltaTime; yield return null; }

        if (req.unloadMenu)
        {
            var menu = SceneManager.GetSceneByName(req.menuSceneName);
            if (menu.IsValid() && menu.isLoaded)
                yield return SceneManager.UnloadSceneAsync(req.menuSceneName);
        }

        if (req.loadMenu)
        {
        #if UNITY_EDITOR
            if (!Application.CanStreamedLevelBeLoaded(req.menuSceneName))
                Debug.LogWarning($"[TODO] La scène menu '{req.menuSceneName}' n'est pas encore disponible.");
            else
        #endif
            {
                var add = SceneManager.LoadSceneAsync(req.menuSceneName, LoadSceneMode.Additive);
                if (add != null) while (!add.isDone) yield return null;
            }
        }

        if (req.useFade && cg) yield return Fade(cg, 1f, 0f, req.fadeDuration);
        op.allowSceneActivation = true;
    }

    IEnumerator Fade(CanvasGroup g, float a, float b, float d)
    {
        float t = 0f;
        while (t < d) { t += Time.unscaledDeltaTime; g.alpha = Mathf.Lerp(a, b, t / d); yield return null; }
        g.alpha = b;
    }
}
