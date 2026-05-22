using System;
using UnityEngine;
using UnityEngine.UIElements;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Match.Events;
using VortexTCG.Scripts.Features.Match.Services;

namespace VortexTCG.Scripts.Features.Match.UI
{
    /// <summary>
    /// Gestionnaire UI des phases du match
    /// Remplace l'ancien PhaseManager en utilisant MatchEvents
    /// </summary>
    public class PhaseUI : MonoBehaviour
    {
        public static PhaseUI Instance { get; private set; }

        [Header("UI Toolkit")]
        [SerializeField] private UIDocument uiDoc;

        private Button endTurnButton;
        private Label matchPhaseLabel;
        private Label turnStatusLabel;
        private Label timerLabel; // Nouveau label pour afficher le timer
        
        // Player HUD elements
        private Label goldLabel;
        private Label secondaryCurrencyLabel;
        private Label championNameLabel;
        private Label championHpLabel;
        private Label championDescriptionLabel;
        
        // Opponent HUD elements
        private Label opponentGoldLabel;
        private Label opponentSecondaryCurrencyLabel;
        private Label opponentChampionNameLabel;
        private Label opponentChampionHpLabel;
        private Label opponentChampionDescriptionLabel;
        private VisualElement switchingPhaseElement;
        
        private float endTurnDefaultOpacity = 1f;
        private Scale endTurnDefaultScale = new Scale(Vector3.one);

        private GamePhase _currentPhase = GamePhase.STAND_BY;
        private bool _canAct;
        private long? _timerEndTime; // Timestamp de fin du timer (Unix ms)
        private const int PhaseFallbackDurationSeconds = 60;
        private const float SwitchingPhaseFadeInSeconds = 0.25f;
        private const float SwitchingPhaseStaticSeconds = 1f;
        private const float SwitchingPhaseFadeOutSeconds = 0.25f;
        private const float SwitchingPhaseDurationSeconds =
            SwitchingPhaseFadeInSeconds + SwitchingPhaseStaticSeconds + SwitchingPhaseFadeOutSeconds;
        private float _switchingPhaseElapsed;
        private bool _isSwitchingPhaseVisible;

        private Texture2D _standByPhaseTexture;
        private Texture2D _attackPhaseTexture;
        private Texture2D _defensePhaseTexture;
        private Texture2D _endingPhaseTexture;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            Debug.Log("[PhaseUI] OnEnable");

            BindUIElements();

            MatchEvents.OnPhaseChanged += HandlePhaseChanged;
            MatchEvents.OnGameStarted += HandleGameStarted;
            MatchEvents.OnPlayerCardPlayed += HandlePlayerCardPlayed;
            MatchEvents.OnOpponentCardPlayed += HandleOpponentCardPlayed;

            SignalRClient client = SignalRClient.Instance;
            if (client != null)
                client.OnMatched += HandleMatched;
        }

        private void OnDisable()
        {
            MatchEvents.OnPhaseChanged -= HandlePhaseChanged;
            MatchEvents.OnGameStarted -= HandleGameStarted;
            MatchEvents.OnPlayerCardPlayed -= HandlePlayerCardPlayed;
            MatchEvents.OnOpponentCardPlayed -= HandleOpponentCardPlayed;

            SignalRClient client = SignalRClient.Instance;
            if (client != null)
                client.OnMatched -= HandleMatched;

            if (endTurnButton != null)
            {
                endTurnButton.clicked -= RequestEndTurnChange;

                endTurnButton.UnregisterCallback<PointerDownEvent>(HandleEndTurnPointerDown);
                endTurnButton.UnregisterCallback<PointerUpEvent>(HandleEndTurnPointerUp);
                endTurnButton.UnregisterCallback<PointerLeaveEvent>(HandleEndTurnPointerLeave);
            }
        }

        private void BindUIElements()
        {
            if (uiDoc == null)
                uiDoc = GetComponent<UIDocument>();

            if (uiDoc == null)
            {
                Debug.LogError("[PhaseUI] UIDocument not found");
                return;
            }

            VisualElement root = uiDoc.rootVisualElement;

            endTurnButton = root.Q<Button>("EndTurnButton");
            if (endTurnButton != null)
            {
                endTurnDefaultOpacity = endTurnButton.resolvedStyle.opacity;
                endTurnDefaultScale = endTurnButton.resolvedStyle.scale;
                endTurnButton.clicked -= RequestEndTurnChange;
                endTurnButton.clicked += RequestEndTurnChange;

                endTurnButton.UnregisterCallback<PointerDownEvent>(HandleEndTurnPointerDown);
                endTurnButton.UnregisterCallback<PointerUpEvent>(HandleEndTurnPointerUp);
                endTurnButton.UnregisterCallback<PointerLeaveEvent>(HandleEndTurnPointerLeave);
                endTurnButton.RegisterCallback<PointerDownEvent>(HandleEndTurnPointerDown);
                endTurnButton.RegisterCallback<PointerUpEvent>(HandleEndTurnPointerUp);
                endTurnButton.RegisterCallback<PointerLeaveEvent>(HandleEndTurnPointerLeave);
                UpdateEndTurnButtonState();
                Debug.Log($"[PhaseUI] EndTurnButton bound name={endTurnButton.name} enabledSelf={endTurnButton.enabledSelf} visible={endTurnButton.visible} pickingMode={endTurnButton.pickingMode}");
            }
            else
            {
                Debug.LogWarning("[PhaseUI] EndTurnButton not found in UI");
            }

            matchPhaseLabel = root.Q<Label>("MatchPhase");
            turnStatusLabel = root.Q<Label>("TurnStatus") ?? root.Q<Label>("TurnStatusLabel");

            timerLabel = root.Q<Label>("TimerLabel");
            if (timerLabel == null)
            {
                Debug.LogWarning("[PhaseUI] TimerLabel not found in UI");
            }

            switchingPhaseElement = root.Q<VisualElement>("SwitchingPhase");
            if (switchingPhaseElement == null)
            {
                Debug.LogWarning("[PhaseUI] SwitchingPhase not found in UI");
            }
            else
            {
                switchingPhaseElement.style.opacity = 0f;
                switchingPhaseElement.style.display = DisplayStyle.None;
                switchingPhaseElement.visible = false;
            }

            // Bind player HUD elements
            goldLabel = root.Q<Label>("P1Golds");
            secondaryCurrencyLabel = root.Q<Label>("SecondaryCurrencyLabel");
            championNameLabel = root.Q<Label>("P1ChampionName") ?? root.Q<Label>("ChampionNameLabel");
            championHpLabel = root.Q<Label>("P1ChampionHP");
            championDescriptionLabel = root.Q<Label>("ChampionDescriptionLabel");

            // Bind opponent HUD elements
            opponentGoldLabel = root.Q<Label>("P2Golds");
            opponentSecondaryCurrencyLabel = root.Q<Label>("OpponentSecondaryCurrencyLabel");
            opponentChampionNameLabel = root.Q<Label>("P2ChampionName") ?? root.Q<Label>("OpponentChampionNameLabel");
            opponentChampionHpLabel = root.Q<Label>("P2ChampionHP");
            opponentChampionDescriptionLabel = root.Q<Label>("OpponentChampionDescriptionLabel");

            Debug.Log($"[PhaseUI] Player HUD elements bound - Gold: {(goldLabel != null ? "OK" : "NULL")}, Secondary: {(secondaryCurrencyLabel != null ? "OK" : "NULL")}, Champion: {(championNameLabel != null ? "OK" : "NULL")}");
            Debug.Log($"[PhaseUI] Opponent HUD elements bound - Gold: {(opponentGoldLabel != null ? "OK" : "NULL")}, Secondary: {(opponentSecondaryCurrencyLabel != null ? "OK" : "NULL")}, Champion: {(opponentChampionNameLabel != null ? "OK" : "NULL")}");

            PhaseService phaseService = PhaseService.Instance;
            if (phaseService != null)
            {
                _currentPhase = phaseService.CurrentPhase;
                Debug.Log($"[PhaseUI] Initialized phase from PhaseService: {_currentPhase}");
            }

            SignalRClient client = SignalRClient.Instance;
            if (client != null && client.PlayerPosition > 0)
            {
                // Fallback when the initial phase event was fired before this UI subscribed.
                _canAct = client.PlayerPosition == 1;
                Debug.Log($"[PhaseUI] Fallback canAct from player position: pos={client.PlayerPosition} canAct={_canAct}");
            }

            UpdatePhaseLabel(_currentPhase);
            UpdateTurnStatusLabel();
            UpdateEndTurnButtonState();
            ShowSwitchingPhase(_currentPhase);
            
            // Initialize player data
            InitializePlayerData();
        }


        private void HandleGameStarted(PhaseChangeResultDTO result)
        {
            _currentPhase = result.CurrentPhase;
            _canAct = result.CanAct;
            SetTimerFromServerOrFallback(result.TimerEndTime);
            UpdatePhaseLabel(_currentPhase);
            UpdateTurnStatusLabel();
            UpdateEndTurnButtonState();
            ShowSwitchingPhase(_currentPhase);
            RefreshResourceDisplays();
            Debug.Log($"[PhaseUI] Game started - Phase: {_currentPhase}, TimerEndTime: {_timerEndTime}");
        }

        private void HandlePhaseChanged(PhaseChangeResultDTO result)
        {
            _currentPhase = result.CurrentPhase;
            _canAct = result.CanAct;
            SetTimerFromServerOrFallback(result.TimerEndTime);
            UpdatePhaseLabel(_currentPhase);
            UpdateTurnStatusLabel();
            UpdateEndTurnButtonState();
            ShowSwitchingPhase(_currentPhase);
            RefreshResourceDisplays();
            Debug.Log($"[PhaseUI] Phase changed - New phase: {_currentPhase}, TimerEndTime: {_timerEndTime}");
        }

        private void HandlePlayerCardPlayed(PlayCardPlayerResultDto result)
        {
            RefreshResourceDisplays();
        }

        private void HandleOpponentCardPlayed(PlayCardOpponentResultDto result)
        {
            RefreshResourceDisplays();
        }

        private void HandleMatched(string _)
        {
            InitializePlayerData();
        }

        private void UpdateTurnStatusLabel()
        {
            if (turnStatusLabel == null)
                return;

            turnStatusLabel.text = _canAct ? "TON TOUR" : "TOUR ADVERSE";
            turnStatusLabel.style.color = _canAct
                ? new StyleColor(new Color(0.3176f, 0.5569f, 0.9490f))
                : new StyleColor(new Color(0.9333f, 0.2157f, 0.2549f));
        }

        private void SetTimerFromServerOrFallback(long? serverTimerEndTime)
        {
            if (serverTimerEndTime.HasValue)
            {
                _timerEndTime = serverTimerEndTime;
                return;
            }

            long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _timerEndTime = nowMs + (PhaseFallbackDurationSeconds * 1000L);
            Debug.Log($"[PhaseUI] Server timer missing, using local fallback ({PhaseFallbackDurationSeconds}s) end={_timerEndTime}");
        }

        private void RequestEndTurnChange()
        {
            Debug.Log($"[PhaseUI] RequestEndTurnChange received canAct={_canAct} buttonEnabled={(endTurnButton != null ? endTurnButton.enabledSelf : false)}");

            if (!_canAct)
            {
                Debug.LogWarning("[PhaseUI] Local canAct is false, but sending ChangePhase anyway and letting server validate turn ownership");
            }

            Debug.Log("[PhaseUI] End Phase button clicked - requesting phase change");
            RestoreEndTurnEffect();
            
            SignalRClient client = SignalRClient.Instance;
            if (client != null && client.IsConnected)
            {
                _ = client.ChangePhase();
                Debug.Log("[PhaseUI] ✅ ChangePhase request sent to server");
            }
            else
            {
                Debug.LogWarning("[PhaseUI] ❌ Cannot change phase - SignalRClient not connected");
        }
        }

        private void HandleEndTurnPointerDown(PointerDownEvent evt)
        {
            if (endTurnButton == null) return;
            Debug.Log($"[PhaseUI] PointerDown on EndTurnButton pointerId={evt.pointerId} position={evt.position} canAct={_canAct}");
            endTurnButton.style.opacity = 0.9f;
            endTurnButton.style.scale = new Scale(new Vector3(0.97f, 0.97f, 1f));
        }

        private void HandleEndTurnPointerUp(PointerUpEvent evt)
        {
            Debug.Log($"[PhaseUI] PointerUp on EndTurnButton pointerId={evt.pointerId} position={evt.position} canAct={_canAct}");
            RestoreEndTurnEffect();
        }

        private void HandleEndTurnPointerLeave(PointerLeaveEvent evt)
        {
            Debug.Log($"[PhaseUI] PointerLeave on EndTurnButton pointerId={evt.pointerId} canAct={_canAct}");
            RestoreEndTurnEffect();
        }

        private void RestoreEndTurnEffect()
        {
            if (endTurnButton == null) return;
            endTurnButton.style.opacity = endTurnDefaultOpacity;
            endTurnButton.style.scale = endTurnDefaultScale;
        }

        private void UpdateEndTurnButtonState()
        {
            if (endTurnButton == null) return;

            endTurnButton.SetEnabled(true);
            endTurnButton.style.opacity = _canAct ? endTurnDefaultOpacity : 0.75f;
            Debug.Log($"[PhaseUI] UpdateEndTurnButtonState canAct={_canAct} enabledSelf={endTurnButton.enabledSelf} (forced enabled) opacity={endTurnButton.style.opacity.value}");
        }

        private void UpdatePhaseLabel(GamePhase phase)
        {
            if (matchPhaseLabel == null) return;

            string label = phase switch
            {
                GamePhase.STAND_BY => "STAND BY PHASE",
                GamePhase.ATTACK => "ATTACK PHASE",
                GamePhase.DEFENSE => "DEFENSE PHASE",
                GamePhase.END_TURN => "END TURN PHASE",
                _ => "PHASE"
            };

            matchPhaseLabel.text = label;
        }

        // ========== TIMER ==========

        private void Update()
        {
            UpdateTimerDisplay();
            UpdateSwitchingPhaseFade();
        }

        private void ShowSwitchingPhase(GamePhase phase)
        {
            if (switchingPhaseElement == null)
                return;

            Texture2D phaseTexture = GetSwitchingPhaseTexture(phase);
            if (phaseTexture == null)
            {
                Debug.LogWarning($"[PhaseUI] Missing texture for switching phase '{phase}'");
                return;
            }

            switchingPhaseElement.style.backgroundImage = new StyleBackground(phaseTexture);
            switchingPhaseElement.style.opacity = 0f;
            switchingPhaseElement.style.display = DisplayStyle.Flex;
            switchingPhaseElement.visible = true;

            _switchingPhaseElapsed = 0f;
            _isSwitchingPhaseVisible = true;
        }

        private void UpdateSwitchingPhaseFade()
        {
            if (!_isSwitchingPhaseVisible || switchingPhaseElement == null)
                return;

            _switchingPhaseElapsed += Time.deltaTime;
            float elapsed = _switchingPhaseElapsed;
            float fadeInEnd = SwitchingPhaseFadeInSeconds;
            float staticEnd = fadeInEnd + SwitchingPhaseStaticSeconds;
            float fadeOutEnd = staticEnd + SwitchingPhaseFadeOutSeconds;

            float opacity;
            if (elapsed <= fadeInEnd)
            {
                opacity = Mathf.Clamp01(elapsed / SwitchingPhaseFadeInSeconds);
            }
            else if (elapsed <= staticEnd)
            {
                opacity = 1f;
            }
            else
            {
                float fadeOutProgress = (elapsed - staticEnd) / SwitchingPhaseFadeOutSeconds;
                opacity = Mathf.Clamp01(1f - fadeOutProgress);
            }

            switchingPhaseElement.style.opacity = opacity;

            if (_switchingPhaseElapsed >= SwitchingPhaseDurationSeconds)
            {
                _isSwitchingPhaseVisible = false;
                switchingPhaseElement.style.opacity = 0f;
                switchingPhaseElement.style.display = DisplayStyle.None;
                switchingPhaseElement.visible = false;
            }
        }

        private Texture2D GetSwitchingPhaseTexture(GamePhase phase)
        {
            return phase switch
            {
                GamePhase.STAND_BY => _standByPhaseTexture ??= Resources.Load<Texture2D>("GUI/StandByPhase"),
                GamePhase.ATTACK => _attackPhaseTexture ??= Resources.Load<Texture2D>("GUI/AttackPhase"),
                GamePhase.DEFENSE => _defensePhaseTexture ??= Resources.Load<Texture2D>("GUI/DefensePhase"),
                GamePhase.END_TURN => _endingPhaseTexture ??= Resources.Load<Texture2D>("GUI/EndingPhase"),
                _ => null
            };
        }

        private void UpdateTimerDisplay()
        {
            if (timerLabel == null) return;

            if (_timerEndTime == null)
            {
                timerLabel.text = $"00:{PhaseFallbackDurationSeconds:D2}";
                timerLabel.style.color = new StyleColor(Color.white);
                return;
            }

            long currentTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long remainingMs = _timerEndTime.Value - currentTimeMs;

            if (remainingMs <= 0)
            {
                timerLabel.style.color = new StyleColor(Color.red);
            }
            else
            {
                int totalSeconds = Mathf.CeilToInt(remainingMs / 1000f);
                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;

                timerLabel.text = $"{minutes:D2}:{seconds:D2}";

                if (totalSeconds <= 10)
                {
                    timerLabel.style.color = new StyleColor(Color.red);
                }
                else if (totalSeconds <= 30)
                {
                    timerLabel.style.color = new StyleColor(new Color(1f, 0.5f, 0f));
                }
                else
                {
                    timerLabel.style.color = new StyleColor(Color.white);
                }
            }
        }

        // ========== PLAYER HUD ==========

        private void InitializePlayerData()
        {
            SignalRClient client = SignalRClient.Instance;
            if (client == null)
            {
                Debug.LogWarning("[PhaseUI] SignalRClient not available for player data initialization");
                return;
            }

            // Initialize player data
            InitializePlayerHUD(client);
            
            // Initialize opponent data
            InitializeOpponentHUD(client);

            Debug.Log($"[PhaseUI] Player and opponent data initialized");
        }

        private void InitializePlayerHUD(SignalRClient client)
        {
            MatchInitChampionDto championForP1 = client.Position1Champion;
            if (championForP1 == null)
            {
                championForP1 = client.PlayerPosition == 1 ? client.PlayerChampion : client.OpponentChampion;
            }

            // Set champion info
            if (championForP1 != null)
            {
                if (championNameLabel != null)
                    championNameLabel.text = championForP1.Name;

                if (championDescriptionLabel != null)
                    championDescriptionLabel.text = championForP1.Description;

                if (championHpLabel != null)
                    championHpLabel.text = $"HP : {championForP1.Hp}";

                Debug.Log($"[PhaseUI] P1 champion initialized: {championForP1.Name}");
            }

            // Set initial resources
            UpdatePlayerGoldDisplay(client.PlayerGold);
            UpdatePlayerSecondaryCurrencyDisplay(client.SecondaryCurrencyName, client.PlayerSecondaryCurrency);

            Debug.Log($"[PhaseUI] Player data initialized - Gold: {client.PlayerGold}, Secondary: {client.PlayerSecondaryCurrency} ({client.SecondaryCurrencyName})");
        }

        private void InitializeOpponentHUD(SignalRClient client)
        {
            MatchInitChampionDto championForP2 = client.Position2Champion;
            if (championForP2 == null)
            {
                championForP2 = client.PlayerPosition == 2 ? client.PlayerChampion : client.OpponentChampion;
            }

            // Set champion info
            if (championForP2 != null)
            {
                if (opponentChampionNameLabel != null)
                    opponentChampionNameLabel.text = championForP2.Name;

                if (opponentChampionDescriptionLabel != null)
                    opponentChampionDescriptionLabel.text = championForP2.Description;

                if (opponentChampionHpLabel != null)
                    opponentChampionHpLabel.text = $"HP : {championForP2.Hp}";

                Debug.Log($"[PhaseUI] P2 champion initialized: {championForP2.Name}");
            }

            // Set initial resources
            UpdateOpponentGoldDisplay(client.OpponentGold);
            UpdateOpponentSecondaryCurrencyDisplay(client.OpponentSecondaryCurrencyName, client.OpponentSecondaryCurrency);

            Debug.Log($"[PhaseUI] Opponent data initialized - Gold: {client.OpponentGold}, Secondary: {client.OpponentSecondaryCurrency} ({client.OpponentSecondaryCurrencyName})");
        }

        public void RefreshChampionHpDisplay()
        {
            SignalRClient client = SignalRClient.Instance;
            if (client == null)
            {
                Debug.LogWarning("[PhaseUI] Cannot refresh champion HP display: SignalRClient not available");
                return;
            }

            if (client.Position1Champion != null && championHpLabel != null)
            {
                championHpLabel.text = $"HP : {client.Position1Champion.Hp}";
            }

            if (client.Position2Champion != null && opponentChampionHpLabel != null)
            {
                opponentChampionHpLabel.text = $"HP : {client.Position2Champion.Hp}";
            }
        }

        private void UpdateGoldDisplay(int gold)
        {
            if (goldLabel != null)
            {
                goldLabel.text = $"{gold}";
                Debug.Log($"[PhaseUI] Gold updated: {gold}");
            }
        }

        private void UpdateSecondaryCurrencyDisplay(string currencyName, int amount)
        {
            if (secondaryCurrencyLabel != null)
            {
                secondaryCurrencyLabel.text = $"{currencyName}: {amount}";
                Debug.Log($"[PhaseUI] Secondary currency updated: {currencyName} = {amount}");
            }
        }

        private void UpdatePlayerGoldDisplay(int gold)
        {
            if (goldLabel != null)
            {
                goldLabel.text = $"{gold}";
                Debug.Log($"[PhaseUI] Player gold updated: {gold}");
            }
        }

        private void UpdatePlayerSecondaryCurrencyDisplay(string currencyName, int amount)
        {
            if (secondaryCurrencyLabel != null)
            {
                secondaryCurrencyLabel.text = $"{currencyName}: {amount}";
                Debug.Log($"[PhaseUI] Player secondary currency updated: {currencyName} = {amount}");
            }
        }

        private void UpdateOpponentGoldDisplay(int gold)
        {
            if (opponentGoldLabel != null)
            {
                opponentGoldLabel.text = $"{gold}";
                Debug.Log($"[PhaseUI] Opponent gold updated: {gold}");
            }
        }

        private void UpdateOpponentSecondaryCurrencyDisplay(string currencyName, int amount)
        {
            if (opponentSecondaryCurrencyLabel != null)
            {
                opponentSecondaryCurrencyLabel.text = $"{currencyName}: {amount}";
                Debug.Log($"[PhaseUI] Opponent secondary currency updated: {currencyName} = {amount}");
            }
        }

        private void RefreshResourceDisplays()
        {
            SignalRClient client = SignalRClient.Instance;
            if (client == null)
            {
                return;
            }

            UpdatePlayerGoldDisplay(client.PlayerGold);
            UpdatePlayerSecondaryCurrencyDisplay(client.SecondaryCurrencyName, client.PlayerSecondaryCurrency);
            UpdateOpponentGoldDisplay(client.OpponentGold);
            UpdateOpponentSecondaryCurrencyDisplay(client.OpponentSecondaryCurrencyName, client.OpponentSecondaryCurrency);
        }

        // Public methods to update from external sources (like server events)
        public void UpdateGold(int newGold)
        {
            UpdatePlayerGoldDisplay(newGold);
        }

        public void UpdateSecondaryCurrency(int newAmount)
        {
            SignalRClient client = SignalRClient.Instance;
            if (client != null)
            {
                UpdatePlayerSecondaryCurrencyDisplay(client.SecondaryCurrencyName, newAmount);
            }
        }

        public void UpdateOpponentGold(int newGold)
        {
            UpdateOpponentGoldDisplay(newGold);
        }

        public void UpdateOpponentSecondaryCurrency(int newAmount)
        {
            SignalRClient client = SignalRClient.Instance;
            if (client != null)
            {
                UpdateOpponentSecondaryCurrencyDisplay(client.OpponentSecondaryCurrencyName, newAmount);
            }
        }

    }
}
