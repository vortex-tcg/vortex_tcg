using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Service de surveillance de l'authentification.
/// Vérifie régulièrement la validité du token et redirige vers LoginScene si expiré/invalide.
/// </summary>
public class AuthWatcher : MonoBehaviour
{
    public static AuthWatcher Instance;

    [Tooltip("Référence au JwtStore")]
    [SerializeField] private JwtStore jwtStore;

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
        
        // Trouver JwtStore si pas assigné
        if (jwtStore == null)
            jwtStore = Resources.Load<JwtStore>("JwtStore") ?? FindObjectOfType<JwtStore>();
        
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
        // S'abonner aux changements d'authentification
        if (jwtStore != null)
            jwtStore.OnAuthenticationStatusChanged += OnAuthenticationStatusChanged;
        
        StartCoroutine(Poll());
    }

    /// <summary>Appelé quand le statut d'authentification change</summary>
    private void OnAuthenticationStatusChanged(AuthenticationStatus status)
    {
        // Si le token expire ou l'auth échoue sur une scène protégée
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
        if (jwtStore == null) return;

        var current = SceneManager.GetActiveScene().name;
        if (IsUnguarded(current)) return;

        var authState = jwtStore.GetAuthenticationState();
        
        // Déconnecter si pas d'authentification ou token expiré
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
        //LoadingScreen.Load("LoginScene", loadMenu: false, unloadMenu: true);
    }
}
