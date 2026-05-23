using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using System.IO;

public class LoadingController : MonoBehaviour
{
    public TMP_Text progressText; 
    public CanvasGroup cg;          

    [Header("Video")]
    [SerializeField] private string loadingVideoResourcePath = "Video/loading";

    private LoadingRequest req;
    private GameObject videoOverlayRoot;
    private RenderTexture videoTexture;
    private VideoPlayer videoPlayer;
    private RawImage videoImage;

    void Start()
    {
        req = Resources.Load<LoadingRequest>("LoadingRequest"); 
        if (!req) { Debug.LogError("LoadingRequest introuvable dans Resources."); return; }

        HideDefaultLoadingUI();
        StartCoroutine(PrepareLoadingVideo());
        if (req.useFade && cg) { cg.alpha = 0f; StartCoroutine(Fade(cg, 0f, 1f, req.fadeDuration)); }
        StartCoroutine(LoadNext());
    }

    private IEnumerator PrepareLoadingVideo()
    {
        VideoClip clip = Resources.Load<VideoClip>(loadingVideoResourcePath);
        if (!clip)
        {
            Debug.LogWarning($"[LoadingController] Vidéo de chargement introuvable dans Resources : {loadingVideoResourcePath}");
            yield break;
        }

        CreateVideoOverlay();
        HideDefaultLoadingUI();

        videoPlayer.clip = clip;
        videoPlayer.isLooping = true;
        videoPlayer.playOnAwake = false;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = videoTexture;

        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
            yield return null;

        videoPlayer.Play();
    }

    private void CreateVideoOverlay()
    {
        if (videoOverlayRoot)
            return;

        videoOverlayRoot = new GameObject("LoadingVideoOverlay");
        DontDestroyOnLoad(videoOverlayRoot);

        Canvas canvas = videoOverlayRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = -100;

        videoOverlayRoot.AddComponent<CanvasScaler>();
        videoOverlayRoot.AddComponent<GraphicRaycaster>();

        GameObject videoPlane = new GameObject("Video");
        videoPlane.transform.SetParent(videoOverlayRoot.transform, false);

        RectTransform rectTransform = videoPlane.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        videoImage = videoPlane.AddComponent<RawImage>();
        videoImage.color = Color.white;

        videoTexture = new RenderTexture(Screen.width, Screen.height, 0);
        videoTexture.Create();
        videoImage.texture = videoTexture;

        videoPlayer = videoOverlayRoot.AddComponent<VideoPlayer>();
    }

    private static void HideDefaultLoadingUI()
    {
        GameObject loadingText = GameObject.Find("LoadingText");
        if (loadingText)
            loadingText.SetActive(false);

        GameObject spinner = GameObject.Find("VortexSpinner");
        if (!spinner)
            return;

        spinner.SetActive(false);
    }

    IEnumerator LoadNext()
    {
        yield return null; 
        string sceneName = NormalizeSceneName(req.targetScene);
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        if (op == null) { Debug.LogError("LoadSceneAsync null. Nom de scène invalide ?"); yield break; }
        op.allowSceneActivation = false;

        float elapsed = 0f;
        while (op.progress < 0.9f)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        while (elapsed < req.minShowTime) { elapsed += Time.unscaledDeltaTime; yield return null; }

        if (req.unloadMenu)
        {
            Scene menu = SceneManager.GetSceneByName(req.menuSceneName);
            if (menu.IsValid() && menu.isLoaded)
                yield return SceneManager.UnloadSceneAsync(req.menuSceneName);
        }

        if (req.loadMenu)
        {

        #if UNITY_EDITOR
            if (!Application.CanStreamedLevelBeLoaded(req.menuSceneName)) {
                Debug.LogWarning($"[TODO] La scène menu Scene/UpperMenuUi n'est pas encore disponible.");
            }
            else

        #endif
            {
                SceneManager.LoadSceneAsync(NormalizeSceneName(req.menuSceneName), LoadSceneMode.Additive);
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

    private static string NormalizeSceneName(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return sceneName;

        return Path.GetFileNameWithoutExtension(sceneName).Replace("\\", "/").Split('/')[^1];
    }

    private void OnDestroy()
    {
        if (videoPlayer)
            videoPlayer.Stop();

        if (videoTexture)
        {
            videoTexture.Release();
            Destroy(videoTexture);
        }

        if (videoOverlayRoot)
            Destroy(videoOverlayRoot);
    }
}
