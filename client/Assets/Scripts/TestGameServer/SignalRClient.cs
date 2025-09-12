using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Http.Connections; 
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class SignalRClient : MonoBehaviour
{
    public static SignalRClient Instance { get; private set; }

    [Header("Hub URL (ASP.NET)")]
    [Tooltip("Sera remplacé par la valeur de application-properties.json au démarrage.")]
    public string hubUrl = "https://localhost:5003/hubs/game";

    [Header("Options")]
    public bool autoConnectOnStart = false;
    public string defaultPlayerName = "UnityPlayer";
    [Tooltip("En dev, LongPolling contourne la plupart des soucis WS/SSL/proxy.")]
    public bool forceLongPollingInEditor = true;

    private HubConnection _conn;
    private string _accessToken;
    private string _currentKeyOrCode;  
    private string _mode;              
    public event Action<string> OnStatus;
    public event Action<string> OnLog;
    public event Action<string> OnMatched;
    public event Action OnOpponentLeft;

    private readonly ConcurrentQueue<Action> _main = new();
    private void Enqueue(Action a) => _main.Enqueue(a);
    private void Update() { while (_main.TryDequeue(out var a)) a(); }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        try
        {
            var cfg = ConfigLoader.Load();
            var url = ConfigLoader.BuildGameHubUrl(cfg);
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
        if (autoConnectOnStart)
            await ConnectAndIdentify(defaultPlayerName);
    }

    private async void OnApplicationQuit()
    {
        if (_conn != null) await _conn.DisposeAsync();
    }

    public void SetAuthToken(string token) => _accessToken = token;
    public async Task ConnectAndIdentify(string playerName)
    {
        await EnsureConnected(playerName);
        await SafeSend("SetName", string.IsNullOrWhiteSpace(playerName) ? defaultPlayerName : playerName);
    }

    public async Task EnsureConnected(string playerName)
    {
        if (_conn != null && _conn.State == HubConnectionState.Connected) return;

        _conn = BuildConnection();

        _conn.On<string>("Connected", id => Enqueue(() =>
        {
            OnStatus?.Invoke($"Connecté: {id}");
            OnLog?.Invoke("Connecté au hub.");
        }));

        _conn.On("Waiting", () => Enqueue(() => OnStatus?.Invoke("En attente d'un adversaire...")));

        _conn.On<string, object>("Matched", (key, payloadObj) => Enqueue(() =>
        {
            _currentKeyOrCode = key;
            OnMatched?.Invoke(key);
            OnLog?.Invoke($"Match trouvé ! Salle: {key}");
        }));

        _conn.On<string, string, string>("ReceiveRoomMessage", (key, from, text) =>
            Enqueue(() => OnLog?.Invoke($"{from}: {text}")));

        _conn.On<string>("RoomCreated", code => Enqueue(() =>
        {
            _mode = "code";
            _currentKeyOrCode = code;
            OnLog?.Invoke($"Salle créée. Code: {code}");
            OnStatus?.Invoke($"Salle {code} créée. En attente d'un joueur...");
        }));

        _conn.On<string>("RoomCreateError", reason => Enqueue(() =>
            OnStatus?.Invoke(reason == "CODE_TAKEN" ? "Code déjà pris." : "Erreur création salle.")));

        _conn.On<string>("RoomJoinError", reason => Enqueue(() =>
            OnStatus?.Invoke(reason == "ROOM_FULL" ? "Salle pleine." : "Salle introuvable.")));

        _conn.On<string>("OpponentLeft", _ => Enqueue(() =>
        {
            OnOpponentLeft?.Invoke();
            OnLog?.Invoke(" L'adversaire a quitté.");
        }));

        try
        {
            Debug.Log("[SignalR] Connecting… " + hubUrl);
            await _conn.StartAsync();
            Debug.Log("[SignalR] Connected.");
        }
        catch (Exception ex)
        {
            Enqueue(() =>
            {
                OnStatus?.Invoke("Erreur de connexion (voir Console).");
                Debug.LogError("[SignalR] StartAsync FAILED: " + ex);
            });
        }
    }

    private HubConnection BuildConnection()
    {
        Debug.Log("[SignalR] BuildConnection hubUrl=" + hubUrl);

        var builder = new HubConnectionBuilder()
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

        var a = (args != null && args.Length > 0) ? args : Array.Empty<object>();
        try { await _conn.SendAsync(method, a); }
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
        var a = (args != null && args.Length > 0) ? args : Array.Empty<object>();
        try { await _conn.InvokeAsync(method, a); }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            OnStatus?.Invoke($"Invoke {method} a échoué.");
        }
    }

    public async Task JoinQueue()
    {
        RequireConnectedOrThrow();
        _mode = "queue";
        await _conn.SendAsync("JoinQueue"); 
    }

    public async Task LeaveQueue()
    {
        if (_conn == null) return;
        await SafeSend("LeaveQueue");
        _currentKeyOrCode = null;
        OnLog?.Invoke("Quitte la file/room (matchmaking).");
    }

    public async Task CreateRoom(string preferredCode = null)
    {
        RequireConnectedOrThrow();
        _mode = "code";
        await SafeSend("CreateRoom", string.IsNullOrWhiteSpace(preferredCode) ? null : preferredCode);
    }

    public async Task JoinRoom(string code)
    {
        RequireConnectedOrThrow();
        _mode = "code";
        await SafeSend("JoinRoom", code?.Trim());
    }

    public async Task LeaveRoomByCode()
    {
        if (_conn == null) return;
        await SafeSend("LeaveRoomByCode");
        _currentKeyOrCode = null;
        OnLog?.Invoke("Quitte la room (code).");
    }

    public async Task SendMessageToPeer(string text)
    {
        if (string.IsNullOrWhiteSpace(_currentKeyOrCode)) return;
        if (_mode == "code")
            await SafeSend("SendRoomMessageByCode", _currentKeyOrCode, text);
        else
            await SafeSend("SendRoomMessage", _currentKeyOrCode, text);
        OnLog?.Invoke($"Moi: {text}");
    }

    public async Task PlayCard(int cardId)
    {
        if (string.IsNullOrWhiteSpace(_currentKeyOrCode)) return;
        await SafeSend("PlayCard", _currentKeyOrCode, cardId);
        OnLog?.Invoke($"(Action) PlayCard {cardId}");
    }

    public bool IsConnected => _conn != null && _conn.State == HubConnectionState.Connected;
    public string CurrentKeyOrCode => _currentKeyOrCode;
    public string Mode => _mode; 
}
