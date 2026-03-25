using UnityEngine;
using UnityEngine.SceneManagement;

public class AuthWatcher : MonoBehaviour
{
    public static AuthWatcher Instance;

    [SerializeField] private JwtStore jwtStore;
    public float checkInterval = 3f;
    public int expiryLeewaySeconds = 15;
    public string[] unguardedScenes = { "LoginScene", "LoadingScene" };

    private bool _redirecting;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureAuthWatcher()
    {
        if (Instance != null) return;
        GameObject go = new GameObject("AuthWatcher");
        Instance = go.AddComponent<AuthWatcher>();
        Object.DontDestroyOnLoad(go);
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (jwtStore == null)
            jwtStore = Resources.Load<JwtStore>("JwtStore") ?? FindFirstObjectByType<JwtStore>();
        
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
            if (jwtStore != null)
                jwtStore.OnAuthenticationStatusChanged -= OnAuthenticationStatusChanged;
        }
    }

    void Start()
    {
        if (jwtStore != null)
            jwtStore.OnAuthenticationStatusChanged += OnAuthenticationStatusChanged;
        
        StartCoroutine(Poll());
    }

    private void OnAuthenticationStatusChanged(AuthenticationStatus status)
    {

        if (status.State == AuthenticationState.TokenExpired || 
            status.State == AuthenticationState.AuthenticationFailed)
        {
            if (!IsUnguarded(SceneManager.GetActiveScene().name))
            {
                ForceLogout();
            }
        }
    }

    private System.Collections.IEnumerator Poll()
    {
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(checkInterval);
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
        foreach (string s in unguardedScenes)
            if (string.Equals(s, sceneName))
                return true;
        return false;
    }

    public void CheckAuthNow()
    {
        if (_redirecting) return;
        if (jwtStore == null) return;
        string current = SceneManager.GetActiveScene().name;
        if (IsUnguarded(current)) return;
        AuthenticationState authState = jwtStore.GetAuthenticationState();
        if (authState == AuthenticationState.Unauthenticated || 
            authState == AuthenticationState.TokenExpired ||
            authState == AuthenticationState.AuthenticationFailed)
        {
            ForceLogout();
        }
    }

    public void ForceLogout()
    {
        if (_redirecting) return;
        _redirecting = true;

        if (jwtStore != null)
            jwtStore.Clear();
        
        Debug.Log("[AuthWatcher] Déconnexion forcée - redirection vers LoginScene");
    }
}
