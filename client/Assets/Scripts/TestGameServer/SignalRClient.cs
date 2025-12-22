using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using System.Text.Json;

[DefaultExecutionOrder(-1000)]
public class SignalRClient : MonoBehaviour
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

    public event Action<string> OnStatus;
    public event Action<string> OnLog;
    public event Action<string> OnMatched;
    public event Action OnOpponentLeft;
	public event Action<PhaseChangeResultDTO> OnGameStarted;
	public event Action<ChangePhaseResultDTO> OnPhaseChanged;
	
    public event Action<DrawResultForPlayerDto> OnCardsDrawn;
    public event Action<DrawResultForOpponentDto> OnOpponentCardsDrawn;

    private readonly ConcurrentQueue<Action> _main = new();
    private void Enqueue(Action a) => _main.Enqueue(a);
    private void Update() { while (_main.TryDequeue(out Action a)) a(); }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

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
       _conn.On<string, JsonElement>("Matched", (key, payload) => Enqueue(() =>
	   {
    	 _currentKeyOrCode = key;
		 int pos = 0;
    	 if (payload.TryGetProperty("position", out JsonElement posEl) && posEl.TryGetInt32(out int p))
        	pos = p;
         Debug.Log($"[SignalRClient] Matched key={key} pos={pos} networkRefNull={(networkRef == null)}");
    
		 networkRef?.SetMatch(key, pos); 
		 OnMatched?.Invoke(key);
    	 OnLog?.Invoke($"Match trouvé ! Salle: {key} (pos={pos})");
 		if (pos == 1 && !_startGameRequested)
    	{
        	_startGameRequested = true;
        	_ = SafeInvoke("StartGame");
        	OnLog?.Invoke("[SignalR] pos=1 -> StartGame()");
    	}
		 }));


		_conn.On<PhaseChangeResultDTO>("GameStarted", r => Enqueue(() =>
		{
    		OnGameStarted?.Invoke(r);
   			OnLog?.Invoke($"GameStarted: phase={r.CurrentPhase} turn={r.TurnNumber} canAct={r.CanAct}");
		}));

		_conn.On<ChangePhaseResultDTO>("PhaseChanged", r => Enqueue(() =>
		{
    		OnPhaseChanged?.Invoke(r);
    		OnLog?.Invoke($"PhaseChanged: phase={r.ActivePlayerResult?.CurrentPhase} turn={r.ActivePlayerResult?.TurnNumber} turnChanged={r.TurnChanged}");
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
			networkRef?.ResetMatch();
			_startGameRequested = false;
            OnLog?.Invoke("L'adversaire a quitté.");
        }));

        _conn.On<DrawResultForPlayerDto>("CardsDrawn", r => Enqueue(() =>
        {
            Debug.Log($"[SignalRClient]  CardsDrawn reçu. cards={r?.DrawnCards?.Count ?? -1}");
    		Debug.Log("[RAW CardsDrawn] " + r.ToString());

            OnCardsDrawn?.Invoke(r);
        }));

        _conn.On<DrawResultForOpponentDto>("OpponentCardsDrawn", r => Enqueue(() =>
        {
            OnOpponentCardsDrawn?.Invoke(r);
            OnLog?.Invoke($"OpponentCardsDrawn reçu: {r?.CardsDrawnCount ?? 0} cartes");
        }));
		_conn.On<string>("Error", msg => Enqueue(() =>
		{
  			  Debug.LogError("[Hub Error] " + msg);
  			  OnStatus?.Invoke(msg);
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
    public async Task JoinQueue(Guid deckId)
    {
        RequireConnectedOrThrow();
        _mode = "queue";
        Debug.Log($"[SignalRClient] -> JoinQueue({deckId})");
        await SafeInvoke("JoinQueue", deckId);
    }

    public async Task LeaveQueue()
    {
        if (_conn == null) return;
		networkRef?.ResetMatch();
        await SafeSend("LeaveQueue");
        _currentKeyOrCode = null;
        OnLog?.Invoke("Quitte la file/room (matchmaking).");
		_startGameRequested = false;

    }

    public async Task CreateRoom(Guid deckId, string preferredCode = null)
    {
        RequireConnectedOrThrow();
        _mode = "code";
        await SafeInvoke("CreateRoom", deckId, string.IsNullOrWhiteSpace(preferredCode) ? null : preferredCode);
    }
    public async Task JoinRoom(Guid deckId, string code)
    {
        RequireConnectedOrThrow();
        _mode = "code";
        await SafeInvoke("JoinRoom", deckId, code?.Trim());
    }

    public async Task LeaveRoomByCode()
    {
        if (_conn == null) return;
        await SafeSend("LeaveRoomByCode");
		networkRef?.ResetMatch();
        _currentKeyOrCode = null;
        OnLog?.Invoke("Quitte la room (code).");
		_startGameRequested = false;

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
	public async Task StartGame()
	{
    	RequireConnectedOrThrow();
    	await SafeInvoke("StartGame");
	}

	public async Task ChangePhase()
	{
    	RequireConnectedOrThrow();
    	await SafeInvoke("ChangePhase");
	}

    public async Task DrawCards(int playerPosition, int amount)
    {
        RequireConnectedOrThrow();

        Debug.Log($"[SignalRClient] -> Invoke DrawCards(pos={playerPosition}, amount={amount})");
        await SafeInvoke("DrawCards", playerPosition, amount);
    }

    public bool IsConnected => _conn != null && _conn.State == HubConnectionState.Connected;
    public string CurrentKeyOrCode => _currentKeyOrCode;
    public string Mode => _mode;
}
