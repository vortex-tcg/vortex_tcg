using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VortexTCG.Scripts.Features.Match.Services;
using VortexTCG.Scripts.Features.Match.Events;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.MatchScene;

namespace VortexTCG.Scripts.MatchScene
{
    /// <summary>
    /// Gère l'interaction UI pour les attaques
    /// Wrapper MonoBehaviour autour d'AttackService
    /// À placer dans la scène Unity
    /// </summary>
    public class AttackUI : MonoBehaviour
    {
        public static AttackUI Instance { get; private set; }
        public AttackService AttackService { get; private set; }

        [Header("Player 1 Cards on Board")]
        [SerializeField] private List<CardSlotUI> P1BoardSlots = new List<CardSlotUI>();

        private AttackService attackLogic;

        private void Awake()
        {
            Instance = this;
            AttackService = new AttackService(P1BoardSlots);
            attackLogic = AttackService;
        }

        private void Start()
        {
            if (PhaseService.Instance != null)
            {
                PhaseService.Instance.OnEnterAttack += OnEnterAttackPhase;
                PhaseService.Instance.OnEnterDefense += OnEnterDefensePhase;
                PhaseService.Instance.OnEnterStandBy += OnEndDefensePhase;
            }
            if (SignalRClient.Instance != null)
                SignalRClient.Instance.OnAttackEngage += OnAttackEngage;

            attackLogic.RegisterExistingCardsFromSlots();
        }

        private void OnDestroy()
        {
            if (PhaseService.Instance != null)
            {
                PhaseService.Instance.OnEnterAttack -= OnEnterAttackPhase;
                PhaseService.Instance.OnEnterDefense -= OnEnterDefensePhase;
                PhaseService.Instance.OnEnterStandBy -= OnEndDefensePhase;
            }

            if (SignalRClient.Instance != null)
                SignalRClient.Instance.OnAttackEngage -= OnAttackEngage;
        }

        private void OnEnterAttackPhase() => attackLogic.ClearSelections();
        private void OnEnterDefensePhase() { }
        private void OnEndDefensePhase() => attackLogic.ClearSelections();

        public void RegisterCard(CardUI card) => attackLogic.RegisterCard(card);

        public bool IsP1BoardSlot(CardSlotUI slot) => attackLogic.IsP1BoardSlot(slot);

        public bool IsCardOnP1Board(CardUI card) => attackLogic.IsCardOnP1Board(card);

        public void HandleCardClicked(CardUI card)
        {
            if (card == null)
            {
                Debug.LogWarning("[AttackUI] HandleCardClicked: card is NULL");
                return;
            }

            if (PhaseService.Instance == null)
            {
                Debug.LogWarning("[AttackUI] HandleCardClicked: PhaseService.Instance is NULL");
                return;
            }

            Debug.Log($"[AttackUI] Click card name='{card.name}' cardId='{card.cardId}' " +
                      $"phase={PhaseService.Instance.CurrentPhase} onP1Board={IsCardOnP1Board(card)}");

            if (PhaseService.Instance.CurrentPhase != GamePhase.ATTACK)
            {
                Debug.LogWarning("[AttackUI] Not in ATTACK phase -> ignore click");
                return;
            }

            if (!IsCardOnP1Board(card))
            {
                Debug.LogWarning("[AttackUI] Card is NOT on P1 board -> ignore click");
                return;
            }

            if (!int.TryParse(card.cardId, out int cardIdInt))
            {
                Debug.LogError($"[AttackUI] card.cardId not int! value='{card.cardId}'");
                return;
            }

            SignalRClient client = SignalRClient.Instance;
            if (client == null)
            {
                Debug.LogError("[AttackUI] SignalRClient.Instance is NULL");
                return;
            }

            Debug.Log($"[AttackUI] -> calling Hub HandleAttackPos(cardId={cardIdInt})");
            ToggleCard(card);

            _ = SendAttackToServer(client, cardIdInt, card);
        }

        private async Task SendAttackToServer(SignalRClient client, int cardIdInt, CardUI card)
        {
            try
            {
                await client.HandleAttackPos(cardIdInt);
                Debug.Log($"[AttackUI] Hub call HandleAttackPos DONE cardId={cardIdInt}");
            }
            catch (Exception ex)
            {
                ToggleCard(card);
                Debug.LogError($"[AttackUI] Hub call HandleAttackPos FAILED cardId={cardIdInt} ex={ex}");
            }
        }

        private void ToggleCard(CardUI card)
        {
            if (card.IsSelected)
                DeselectCard(card);
            else
                SelectCard(card);
        }

        private void SelectCard(CardUI card)
        {
            card.SetSelected(true);
        }

        private void DeselectCard(CardUI card)
        {
            card.SetSelected(false);
            card.ClearAttackOrder();
        }

        public void ClearSelections() => attackLogic.ClearSelections();

        private void OnAttackEngage(List<int> attackIds) => attackLogic.ApplyAttackStateFromServer(attackIds);
    }
}
