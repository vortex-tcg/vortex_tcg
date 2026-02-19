using System.Collections.Generic;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Match.Services;
using VortexTCG.Scripts.MatchScene;

namespace VortexTCG.Scripts.MatchScene
{
    /// <summary>
    /// Gère l'interaction UI pour la défense
    /// Wrapper MonoBehaviour autour d'DefenseService
    /// À placer dans la scène Unity
    /// </summary>
    public class DefenseUI : MonoBehaviour
    {
        public static DefenseUI Instance { get; private set; }
        public DefenseService DefenseService { get; private set; }

        [SerializeField] private List<CardSlotUI> P1BoardSlots = new List<CardSlotUI>();
        [SerializeField] private List<CardSlotUI> P2BoardSlots = new List<CardSlotUI>();

        private DefenseService defenseLogic;

        private void Awake()
        {
            Instance = this;
            DefenseService = new DefenseService(P1BoardSlots, P2BoardSlots);
            defenseLogic = DefenseService;
        }

        private void Start()
        {
            if (PhaseService.Instance != null)
            {
                PhaseService.Instance.OnEnterDefense += OnEnterDefense;
                PhaseService.Instance.OnEnterStandBy += OnExitDefense;
                PhaseService.Instance.OnEnterAttack += OnExitDefense;
            }

            defenseLogic.RegisterExistingCardsFromSlots();
            if (SignalRClient.Instance != null)
                SignalRClient.Instance.OnDefenseEngage += OnDefenseEngage;
        }

        private void OnDestroy()
        {
            if (PhaseService.Instance != null)
            {
                PhaseService.Instance.OnEnterDefense -= OnEnterDefense;
                PhaseService.Instance.OnEnterStandBy -= OnExitDefense;
                PhaseService.Instance.OnEnterAttack -= OnExitDefense;
            }

            if (SignalRClient.Instance != null)
                SignalRClient.Instance.OnDefenseEngage -= OnDefenseEngage;
        }

        private void OnEnterDefense() => defenseLogic.ClearAllDefense();
        private void OnExitDefense() => defenseLogic.ClearAllDefense();

        public void RegisterCard(CardUI card) => defenseLogic.RegisterCard(card);

        public bool IsP1BoardSlot(CardSlotUI slot) => defenseLogic.IsP1BoardSlot(slot);
        public bool IsP2BoardSlot(CardSlotUI slot) => defenseLogic.IsP2BoardSlot(slot);

        public bool IsCardOnP1Board(CardUI card) => defenseLogic.IsCardOnP1Board(card);
        public bool IsCardOnP2Board(CardUI card) => defenseLogic.IsCardOnP2Board(card);

        public void HandleCardClicked(CardUI card)
        {
            if (card == null) return;
            if (PhaseService.Instance == null) return;
            if (PhaseService.Instance.CurrentPhase != GamePhase.DEFENSE) return;

            CardSlotUI slot = card.GetComponentInParent<CardSlotUI>();
            if (slot == null) return;

            if (IsP1BoardSlot(slot))
            {
                defenseLogic.SelectDefender(card);
                return;
            }

            if (IsP2BoardSlot(slot))
            {
                _ = defenseLogic.TryAssignDefenseAndSend(card);
            }
        }

        public void ClearAllDefense() => defenseLogic.ClearAllDefense();

        private void OnDefenseEngage(DefenseDataResponseDto data) => defenseLogic.ApplyDefenseStateFromServer(data);
    }
}
