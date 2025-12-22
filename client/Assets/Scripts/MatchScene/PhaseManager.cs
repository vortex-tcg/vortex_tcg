using System;
using UnityEngine;
using UnityEngine.UIElements;
using VortexTCG.Scripts.DTOs;

namespace VortexTCG.Scripts.MatchScene
{
    public class PhaseManager : MonoBehaviour
    {
        public static PhaseManager Instance { get; private set; }

        [Header("UI Toolkit")]
        [SerializeField] private UIDocument uiDoc;

        private VisualElement placementIcon;
        private VisualElement attackIcon;
        private VisualElement defenseIcon;
        private VisualElement endTurnIcon;

        public GamePhase CurrentPhase { get; private set; } = GamePhase.PLACEMENT;
        public event Action OnRequestChangePhase;
        public event Action OnEnterPlacement;
        public event Action OnEnterAttack;
        public event Action OnEnterDefense;
        public event Action OnEnterEndTurn;
        public event Action OnEnterStandBy;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            if (uiDoc == null)
                uiDoc = GetComponent<UIDocument>();

            VisualElement root = uiDoc.rootVisualElement;

            Button phaseButton = root.Q<Button>("EndPhaseButton");
            placementIcon = root.Q<VisualElement>("StandBy");
            attackIcon = root.Q<VisualElement>("Attack");
            defenseIcon = root.Q<VisualElement>("Defense");
            endTurnIcon = root.Q<VisualElement>("EndTurn");    

            if (phaseButton != null)
            {
                phaseButton.clicked -= RequestNextPhase;
                phaseButton.clicked += RequestNextPhase;
            }

            UpdateIcons(CurrentPhase);
        }

        private void RequestNextPhase() => OnRequestChangePhase?.Invoke();
        public void SetPhase(GamePhase phase) => ApplyServerPhase(phase);

        public void ApplyServerPhase(GamePhase phase)
        {
            if (CurrentPhase == phase)
            {
                UpdateIcons(CurrentPhase);
                return;
            }

            CurrentPhase = phase;

            switch (CurrentPhase)
            {
                case GamePhase.PLACEMENT:
                    OnEnterPlacement?.Invoke();
                    OnEnterStandBy?.Invoke();
                    break;
                case GamePhase.ATTACK:
                    OnEnterAttack?.Invoke();
                    break;
                case GamePhase.DEFENSE:
                    OnEnterDefense?.Invoke();
                    break;
                case GamePhase.END_TURN:
                    OnEnterEndTurn?.Invoke();
                    break;
            }

            UpdateIcons(CurrentPhase);
        }

        private void UpdateIcons(GamePhase phase)
        {
            SetHighlight(placementIcon, phase == GamePhase.PLACEMENT);
            SetHighlight(attackIcon, phase == GamePhase.ATTACK);
            SetHighlight(defenseIcon, phase == GamePhase.DEFENSE);
            if (endTurnIcon != null)
                SetHighlight(endTurnIcon, phase == GamePhase.END_TURN);
        }

        private static void SetHighlight(VisualElement icon, bool active)
        {
            if (icon == null) return;
            icon.style.opacity = active ? 1f : 0.3f;
        }
    }
}
