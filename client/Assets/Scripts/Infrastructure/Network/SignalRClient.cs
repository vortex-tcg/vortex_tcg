using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

[DefaultExecutionOrder(-1000)]
public partial class SignalRClient : MonoBehaviour
{
    public static SignalRClient Instance { get; private set; }

    [Header("Hub URL (ASP.NET)")]
    [Tooltip("Sera remplacé par la valeur de application-properties.json au démarrage.")]
    public string hubUrl = "https://localhost:5003/hubs/game";
    [SerializeField] private NetworkRef networkRef;
    public void BindNetworkRef(NetworkRef nr) => networkRef = nr;

    [Header("Options")]
    public bool autoConnectOnStart = false;
    public string defaultPlayerName = "UnityPlayer";
    [Tooltip("En dev, LongPolling contourne la plupart des soucis WS/SSL/proxy.")]
    public bool forceLongPollingInEditor = true;

    private HubConnection _conn;
    private string _accessToken;
    private string _currentKeyOrCode;
    private string _mode;
    private bool _startGameRequested;
    private int _playerPosition = -1;

    public event Action<BattlesDataDto, bool> OnBattleResolution;
    public event Action<string> OnStatus;
    public event Action<string> OnLog;
    public event Action<string> OnMatched;
    public event Action OnOpponentLeft;
    public event Action<PhaseChangeResultDTO> OnGameStarted;
    public event Action<PhaseChangeResultDTO> OnPhaseChanged;
    public event Action<DrawResultForPlayerDto> OnCardsDrawn;
    public event Action<DrawResultForOpponentDto> OnOpponentCardsDrawn;
    public event Action<PlayCardPlayerResultDto> OnPlayCardResult;
    public event Action<PlayCardOpponentResultDto> OnOpponentPlayCardResult;
    public event Action<List<int>> OnAttackEngage;
    public event Action<List<int>> OnOpponentAttackEngage;
    public event Action<DefenseDataResponseDto> OnDefenseEngage;
    public event Action<DefenseDataResponseDto> OnOpponentDefenseEngage;

    private readonly ConcurrentQueue<Action> _main = new();
    private void Enqueue(Action a) => _main.Enqueue(a);
    private void Update() { while (_main.TryDequeue(out Action a)) a(); }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create MatchService early so it can subscribe before events are raised.
        EnsureMatchServiceExists();

        try
        {
            AppConfig cfg = ConfigLoader.Load();
            string url = ConfigLoader.BuildGameHubUrl(cfg);
            if (!string.IsNullOrWhiteSpace(url)) hubUrl = url;
            Debug.Log("[SignalR] hubUrl from config = " + hubUrl);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SignalR] Config non chargée, on garde hubUrl='{hubUrl}'. Détails: {e.Message}");
        }
    }

    private async void Start()
    {
        Debug.Log($"[SignalRClient] Start() - autoConnectOnStart={autoConnectOnStart}");
        
        // Ensure MatchService exists
        EnsureMatchServiceExists();
        
        if (autoConnectOnStart)
        {
            Debug.Log("[SignalRClient] ✅ Calling ConnectAndIdentify from Start()");
            await ConnectAndIdentify(defaultPlayerName);
        }
        else
        {
            Debug.Log("[SignalRClient] ⚠️ autoConnectOnStart is FALSE - waiting for external call");
        }
    }

    private void EnsureMatchServiceExists()
    {
        // Check if MatchService already exists
        var matchService = FindObjectOfType<VortexTCG.Scripts.Features.Match.Services.MatchService>();
        
        if (matchService == null)
        {
            Debug.LogWarning("[SignalRClient] MatchService not found in scene - creating one automatically");
            GameObject matchServiceGO = new GameObject("MatchService");
            matchServiceGO.AddComponent<VortexTCG.Scripts.Features.Match.Services.MatchService>();
            Debug.Log("[SignalRClient] ✅ MatchService created successfully");
        }
        else
        {
            Debug.Log("[SignalRClient] ✅ MatchService already exists in scene");
        }
    }

    private async void OnApplicationQuit()
    {
        if (_conn != null) await _conn.DisposeAsync();
    }

    public void SetAuthToken(string token) => _accessToken = token;

    private HubConnection BuildConnection()
    {
        Debug.Log("[SignalR] BuildConnection hubUrl=" + hubUrl);

        IHubConnectionBuilder builder = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                if (!string.IsNullOrEmpty(_accessToken))
                    options.AccessTokenProvider = () => Task.FromResult(_accessToken);

#if UNITY_EDITOR
                options.HttpMessageHandlerFactory = (handler) =>
                {
                    if (handler is HttpClientHandler h)
                        h.ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true;
                    return handler;
                };

                if (forceLongPollingInEditor)
                    options.Transports = HttpTransportType.LongPolling;
#endif
            })
            .WithAutomaticReconnect();

        return builder.Build();
    }

    private void RequireConnectedOrThrow()
    {
        if (_conn == null || _conn.State != HubConnectionState.Connected)
            throw new InvalidOperationException("Pas connecté au hub.");
    }

    private async Task SafeSend(string method, params object[] args)
    {
        if (_conn == null || _conn.State != HubConnectionState.Connected)
        {
            OnStatus?.Invoke("Pas connecté au hub.");
            return;
        }

        object[] a = (args != null && args.Length > 0) ? args : Array.Empty<object>();
        try { await _conn.SendCoreAsync(method, a); }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            OnStatus?.Invoke($"Send {method} a échoué.");
        }
    }

    private async Task SafeInvoke(string method, params object[] args)
    {
        if (_conn == null || _conn.State != HubConnectionState.Connected)
        {
            OnStatus?.Invoke("Pas connecté au hub.");
            return;
        }

        object[] a = (args != null && args.Length > 0) ? args : Array.Empty<object>();
        try { await _conn.InvokeCoreAsync(method, a); }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            OnStatus?.Invoke($"Invoke {method} a échoué.");
        }
    }
}

