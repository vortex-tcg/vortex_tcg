using UnityEngine;
using UnityEngine.SceneManagement;

public class AuthWatcher : MonoBehaviour
{
    public static AuthWatcher Instance;

    [Tooltip("Vérification toutes les N secondes (temps réel)")]
    public float checkInterval = 3f;

    [Tooltip("Marge avant exp (en sec) : si exp <= now + leeway => considéré expiré")]
    public int expiryLeewaySeconds = 15;

    [Tooltip("Scènes non protégées")]
    public string[] unguardedScenes = { "LoginScene", "LoadingScene" };

    private bool _redirecting;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureAuthWatcher()
    {
        if (Instance != null) return;
        var go = new GameObject("AuthWatcher");
        Instance = go.AddComponent<AuthWatcher>();
        Object.DontDestroyOnLoad(go);
    }
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    void Start()
    {
        StartCoroutine(Poll());
    }

    private System.Collections.IEnumerator Poll()
    {
        var wait = new WaitForSecondsRealtime(checkInterval);
        while (true)
        {
            CheckAuthNow();
            yield return wait;
        }
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        CheckAuthNow();
    }

    private bool IsUnguarded(string sceneName)
    {
        foreach (var s in unguardedScenes)
            if (string.Equals(s, sceneName))
                return true;
        return false;
    }

    public void CheckAuthNow()
    {
        if (_redirecting) return;

        var current = SceneManager.GetActiveScene().name;
        if (IsUnguarded(current)) return;

        if (!Jwt.I.IsJwtPresent() || Jwt.I.IsExpired(expiryLeewaySeconds))
        {
            ForceLogout();
        }
    }

    public void ForceLogout()
    {
        if (_redirecting) return;
        _redirecting = true;

        Jwt.I.Clear(); 
        //LoadingScreen.Load("LoginScene", loadMenu: false, unloadMenu: true);

    }
}
